using AvaloniaChatGPT.ChatLogic;
using AvaloniaChatGPT.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;

namespace AvaloniaChatGPT.ViewModels;

/*
 * TODO:
 * - show messages as list (repeated item list?)
 * -- each message asd separate textBox or TextBlock (or something else...)
 * -- text should be selectable
 * -- support markdown
 * - options to save history (json, maybe something more)
 * -- improve UI
 */ 

public class MainViewModel : ViewModelBase
{
    private string _outputText;
    private string _inputText;
    private ObservableCollection<Message> _listOfMessages;
    private readonly ChatHandler _chatHandler;

    public MainViewModel()
    {
        CommandSendMessage = ReactiveCommand.Create(SendMessage);
        _chatHandler = new ChatHandler("");

        _listOfMessages = new ObservableCollection<Message>();
    }

    public string InputText
    {
        get => _inputText;
        set => this.RaiseAndSetIfChanged(ref _inputText, value);
    }

    public string OutputText
    {
        get => _outputText;
        set => this.RaiseAndSetIfChanged(ref _outputText, value);
    }

    public ObservableCollection<Message> ListOfMessages
    {
        get => _listOfMessages;
        set => this.RaiseAndSetIfChanged(ref _listOfMessages, value);
    }

    public ReactiveCommand<Unit, Unit> CommandSendMessage { get; }

    private async void SendMessage()
    {
        // TODO : Add OpenAI moderation

        var msgQuestion = new Message
        {
            Id = Guid.NewGuid(),
            ChatMessage = InputText,
            Time = DateTime.UtcNow,
            Type = MessageType.Out
        };

        _listOfMessages.Add(msgQuestion);
        this.RaisePropertyChanged(nameof(ListOfMessages));

        var response = await _chatHandler.AskQuestion(InputText);

        if (response != null)
        {
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                ChatMessage = response,
                Type = MessageType.In,
                Time = DateTime.UtcNow,
            };
            _listOfMessages.Add(msg);
        }

        this.RaisePropertyChanged(nameof(ListOfMessages));
    }
}
