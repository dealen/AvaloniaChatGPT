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
using System.Text.Json;
using System.IO;
using OpenAI_API;
using System.Collections.Generic;

namespace AvaloniaChatGPT.ViewModels;

/*
 * TODO:
 * 1) encrypt API key saved or file that I am saving
 * - troubleshoot also why does android app now work?
 * 2) message streaming
 * - maybe I need different control - Not ListBox
 * 3) text should be selectable
 * 4) options to save history (json, maybe something more)
 * 5) improve UI
 */

public class MainViewModel : ViewModelBase
{
    private string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvaloniaGPT");
    private string _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvaloniaGPT", "settings.json");
    private string _inputText;
    private bool _isApiWorking;
    private bool _isMainViewVisible;
    private bool _isSettingsVisuble;
    private Message _selectedMessage;
    private ObservableCollection<Message> _listOfMessages;
    private ObservableCollection<ModelItem> _openAIModelList;
    private ChatHandler _chatHandler;
    private AppSettings _settings;
    private AppSettings _settingsEdit;
    private string _providedApiKey;
    private OpenAIAPI _openAiAPI;
    private int _selectedModelIndex;

    public MainViewModel()
    {
        CommandSendMessage = ReactiveCommand.Create(SendMessage);
        CommandRemoveMessage = ReactiveCommand.Create<Message>(RemoveMessage);
        CommandRunSettings = ReactiveCommand.Create(RunSettings);
        CommandSaveSettings = ReactiveCommand.Create(SaveSettings);
        CommandResetSettings = ReactiveCommand.Create(ResetSettings);
        CommandExportChatToJSon = ReactiveCommand.Create(ExportToJSon);
        CommandConfigureAssistant = ReactiveCommand.Create(ConfigureAssistant);

        _listOfMessages = new ObservableCollection<Message>();
        ModelList = new List<ModelItem>
        {
            new ModelItem { Model = Model.ChatGPTTurbo, Name = nameof(Model.ChatGPTTurbo) },
            new ModelItem { Model = Model.ChatGPTTurbo0301, Name = nameof(Model.ChatGPTTurbo0301) },
            new ModelItem { Model = Model.GPT4, Name = nameof(Model.GPT4) },
            new ModelItem { Model = Model.GPT4_32k_Context, Name = nameof(Model.GPT4_32k_Context) },
        };

        OpenAIModelList = new ObservableCollection<ModelItem>(ModelList);
    }

    public OpenAIAPI API
    {
        get => _openAiAPI;
        set => _openAiAPI = value;
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

    public List<ModelItem> ModelList { get; set; }

    public ObservableCollection<ModelItem> OpenAIModelList
    {
        get => _openAIModelList;
        set => this.RaiseAndSetIfChanged(ref _openAIModelList, value);
    }

    public ModelItem SelectedModel
    {
        get => _settings.SelectedModel;
        set => _settings.SelectedModel = value;
    }

    public int SelectedModelIndex
    {
        get => _selectedModelIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedModelIndex, value);
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
    public ReactiveCommand<Unit, Unit> CommandExportChatToJSon { get; }
    public ReactiveCommand<Unit, Unit> CommandConfigureAssistant { get; }

    public async Task InitializeAsync()
    {
        IsMainViewVisible = true;
        IsSettingsVisuble = false;

        await ReadAppSettings();
        _openAiAPI = new OpenAIAPI(_settings.ApiKey);
        IntializeOpenAIApi();
    }

    private async void ConfigureAssistant()
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard("Configure Assistant", "Not yet implemented.");
        await messageBox.ShowAsync();
    }

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

            _chatHandler = new ChatHandler(_openAiAPI);
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
        if (!Directory.Exists(_settingsPath))
        {
            Directory.CreateDirectory(_settingsPath);
        }

        if (!File.Exists(_settingsFilePath))
        {
            _settings = new AppSettings();
            return;
        }

        using (var sr = new StreamReader(_settingsFilePath))
        {
            var deserializedSettings = JsonSerializer.Deserialize<AppSettings>(await sr.ReadToEndAsync());

            if (deserializedSettings is AppSettings appSettings)
            {
                _settings = appSettings;
                ProvidedApiKey = _settings.ApiKey;
                if (_settings.SelectedModel is not null && ModelList is not null && ModelList.Any())
                {
                    SelectedModel = ModelList.FirstOrDefault(x => x.Name == _settings.SelectedModel.Name);
                    if (SelectedModel is not null)
                        SelectedModelIndex = ModelList.IndexOf(SelectedModel);
                }
            }
        }

        await Task.CompletedTask;
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

    private void ExportToJSon()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{DateTime.UtcNow.Ticks}chat.json");
        var serializedSettings = JsonSerializer.Serialize(_listOfMessages);
        File.WriteAllText(path, serializedSettings.ToString());
    }

    private void SaveSettings()
    {
        Settings.ApiKey = ProvidedApiKey;
        Settings.SelectedModel = ModelList[SelectedModelIndex];

        var serializedSettings = JsonSerializer.Serialize(Settings);
        File.WriteAllText(_settingsFilePath, serializedSettings.ToString());

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