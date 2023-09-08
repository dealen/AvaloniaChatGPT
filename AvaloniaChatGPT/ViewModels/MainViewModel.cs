using Avalonia.Threading;
using AvaloniaChatGPT.ChatLogic;
using AvaloniaChatGPT.Models;
using MsBox.Avalonia;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

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
    private string _inputText;
    private bool _isApiWorking;
    private Message _selectedMessage;
    private ObservableCollection<Message> _listOfMessages;
    private bool _isMainViewVisible;
    private bool _isSettingsVisuble;
    private readonly ChatHandler _chatHandler;

    public MainViewModel()
    {
        CommandSendMessage = ReactiveCommand.Create(SendMessage);
        CommandRemoveMessage = ReactiveCommand.Create<Message>(RemoveMessage);
        CommandRunSettings = ReactiveCommand.Create(RunSettings);
        CommandSaveSettings = ReactiveCommand.Create(SaveSettings);
        CommandResetSettings = ReactiveCommand.Create(ResetSettings);

        _chatHandler = new ChatHandler("");

        _listOfMessages = new ObservableCollection<Message>();
    }

    public bool IsMainViewVisible
    {
        get { return _isMainViewVisible; }
        set { this.RaiseAndSetIfChanged(ref _isMainViewVisible, value); }
    }
    public bool IsSettingsVisuble
    {
        get { return _isSettingsVisuble; }
        set { this.RaiseAndSetIfChanged(ref _isSettingsVisuble, value); }
    }

    public bool IsApiWorking
    {
        get { return _isApiWorking; }
        set { this.RaiseAndSetIfChanged(ref _isApiWorking, value); }
    }

    public string InputText
    {
        get => _inputText;
        set => this.RaiseAndSetIfChanged(ref _inputText, value);
    }

    public ObservableCollection<Message> ListOfMessages
    {
        get => _listOfMessages;
        set => this.RaiseAndSetIfChanged(ref _listOfMessages, value);
    }

    public Message SelectedMessage
    {
        get => _selectedMessage;
        set => this.RaiseAndSetIfChanged(ref _selectedMessage, value);
    }

    public ReactiveCommand<Unit, Unit> CommandSendMessage { get; }
    public ReactiveCommand<Message, Unit> CommandRemoveMessage { get; }
    public ReactiveCommand<Unit, Unit> CommandRunSettings { get; }
    public ReactiveCommand<Unit, Unit> CommandSaveSettings { get; }
    public ReactiveCommand<Unit, Unit> CommandResetSettings { get; }

    private async void SendMessage()
    {
        // TODO : Add OpenAI moderation

        if (string.IsNullOrWhiteSpace(InputText))
        {
            var msg = MessageBoxManager.GetMessageBoxStandard("Missing question error", "Please fill question field.");
            await msg.ShowAsync();
            return;
        }

        var msgQuestion = new Message
        {
            Id = Guid.NewGuid(),
            ChatMessage = InputText,
            Time = DateTime.UtcNow,
            Type = MessageType.Out
        };
        InputText = string.Empty;

        _listOfMessages.Add(msgQuestion);
        this.RaisePropertyChanged(nameof(ListOfMessages));

        Dispatcher.UIThread.Post(() => AskAPI(msgQuestion),
                                            DispatcherPriority.Background);
    }

    private async void AskAPI(Message msgQuestion)
    {
        IsApiWorking = true;
        var response = await _chatHandler.AskQuestion(msgQuestion.ChatMessage);

        if (!string.IsNullOrWhiteSpace(response))
        {
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                ChatMessage = response,
                Type = MessageType.In,
                Time = DateTime.UtcNow,
            };
            _listOfMessages.Add(msg);

            this.RaisePropertyChanged(nameof(ListOfMessages));
        }
        IsApiWorking = false;
    }

    private void RemoveMessage(Message message)
    {
        if (message is null)
            return;

        var messageForDeletion = _listOfMessages.FirstOrDefault(x => x.Id == message.Id);
        if (messageForDeletion is null)
            return;

        _listOfMessages.Remove(messageForDeletion);

        this.RaisePropertyChanged(nameof(ListOfMessages));
    }

    private void RunSettings()
    {
        ShowSettings();
    }

    private void ResetSettings()
    {
        HideSettings();
    }

    private void SaveSettings()
    {
        HideSettings();
    }

    private void ShowSettings()
    {
        IsMainViewVisible = false;
        IsSettingsVisuble = true;
    }

    private void HideSettings()
    {
        IsMainViewVisible = true;
        IsSettingsVisuble = false;
    }
}