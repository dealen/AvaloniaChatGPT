using OpenAI_API.Models;
using ReactiveUI;

namespace AvaloniaChatGPT.Models
{
    public class AppSettings : ReactiveObject
    {
        private string _apiKey;
        private Model _selectedModel = OpenAI_API.Models.Model.ChatGPTTurbo;

        public string ApiKey
        {
            get => _apiKey;
            set => this.RaiseAndSetIfChanged(ref _apiKey, value);
        }

        public Model SelectedModel
        {
            get => _selectedModel;
            set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
        }
    }
}
