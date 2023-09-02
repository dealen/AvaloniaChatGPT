using AvaloniaChatGPT.ChatLogic;
using ReactiveUI;
using System.Reactive;

namespace AvaloniaChatGPT.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _outputText;
    private readonly ChatHandler _chatHandler;

    public MainViewModel()
    {
        CommandSendMessage = ReactiveCommand.Create(SendMessage);
        _chatHandler = new ChatHandler("");
    }

    private string _inputText;

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

    public ReactiveCommand<Unit, Unit> CommandSendMessage { get; }

    private async void SendMessage()
    {
        var response = await _chatHandler.AskQuestion(InputText);

        if (response != null)
        {
            OutputText = response;
        }
    }
}
