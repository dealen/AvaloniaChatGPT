using Avalonia.Threading;
using AvaloniaChatGPT.ChatLogic;
using AvaloniaChatGPT.Models;
using MsBox.Avalonia;
using OpenAI_API.Models;
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
    private bool _isMainViewVisible;
    private bool _isSettingsVisuble;
    private Message _selectedMessage;
    private ObservableCollection<Message> _listOfMessages;
    private ObservableCollection<Model> _openAIModelList;
    private ChatHandler _chatHandler;
    private AppSettings _settings;
    private AppSettings _settingsEdit;
    private string _providedApiKey;

    public MainViewModel()
    {
        CommandSendMessage = ReactiveCommand.Create(SendMessage);
        CommandRemoveMessage = ReactiveCommand.Create<Message>(RemoveMessage);
        CommandRunSettings = ReactiveCommand.Create(RunSettings);
        CommandSaveSettings = ReactiveCommand.Create(SaveSettings);
        CommandResetSettings = ReactiveCommand.Create(ResetSettings);

        _listOfMessages = new ObservableCollection<Message>();
    }

    public async Task InitializeAsync()
    {
        IsMainViewVisible = true;
        IsSettingsVisuble = false;

        await ReadAppSettings();
        IntializeOpenAIApi();
    }

    private void IntializeOpenAIApi()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            RunSettings();
            return;
        }
    }

    private async Task ReadAppSettings()
    {
        // TODO not decided how it will be stored but here app will read settings
        _settings = new AppSettings();
        await Task.CompletedTask;
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

    public ObservableCollection<Model> OpenAIModelList
    {
        get => _openAIModelList;
        set => this.RaiseAndSetIfChanged(ref _openAIModelList, value);
    }

    public AppSettings Settings
    {
        get => _settings;
        set => this.RaiseAndSetIfChanged(ref _settings, value);
    }

    public string ProvidedApiKey
    {
        get => _providedApiKey;
        set => this.RaiseAndSetIfChanged(ref _providedApiKey, value);
    }

    public ReactiveCommand<Unit, Unit> CommandSendMessage { get; }
    public ReactiveCommand<Message, Unit> CommandRemoveMessage { get; }
    public ReactiveCommand<Unit, Unit> CommandRunSettings { get; }
    public ReactiveCommand<Unit, Unit> CommandSaveSettings { get; }
    public ReactiveCommand<Unit, Unit> CommandResetSettings { get; }

    private async void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            await ShowMessage("Missing question error", "Please fill question field.");
            return;
        }

        if (_chatHandler is null)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                await ShowMessage("Missing API KEY", "Please provide OpenAI API Key in order to use this application.");

                RunSettings();
                return;
            }

            _chatHandler = new ChatHandler(_settings.ApiKey);
        }

        ProcessUserInput();
    }

    private void ProcessUserInput()
    {
        var msgQuestion = new Message
        {
            Id = Guid.NewGuid(),
            ChatMessage = InputText,
            Time = DateTime.UtcNow,
            Type = MessageType.Out
        };

        _chatHandler.Moderate(msgQuestion.ChatMessage);

        InputText = string.Empty;


        _listOfMessages.Add(msgQuestion);
        this.RaisePropertyChanged(nameof(ListOfMessages));

        Dispatcher.UIThread.Post(() => AskAPI(msgQuestion),
                                            DispatcherPriority.Background);
    }

    private async Task ShowMessage(string v1, string v2)
    {
        var msg = MessageBoxManager.GetMessageBoxStandard("Missing API KEY", "Please provide OpenAI API Key in order to use this application.");
        await msg.ShowAsync();
    }

    private void CheckIfThereIsAnyQuestionContent()
    {
        throw new NotImplementedException();
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
        _settingsEdit = new AppSettings()
        {
            ApiKey = _settings.ApiKey,
            SelectedModel = _settings.SelectedModel,
        };

        ShowSettings();
    }

    private void ResetSettings()
    {
        HideSettings();
    }

    private void SaveSettings()
    {
        Settings.ApiKey = ProvidedApiKey;

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