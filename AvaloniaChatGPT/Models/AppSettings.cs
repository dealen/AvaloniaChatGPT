using OpenAI_API.Models;
using ReactiveUI;

namespace AvaloniaChatGPT.Models
{
    public class AppSettings : ReactiveObject
    {
        private string _apiKey;
        private ModelItem _selectedModel = new ModelItem
        {
            Model = Model.ChatGPTTurbo,
            Name = nameof(Model.ChatGPTTurbo)
        };

        public string ApiKey
        {
            get => _apiKey;
            set => this.RaiseAndSetIfChanged(ref _apiKey, value);
        }

        public ModelItem SelectedModel
        {
            get => _selectedModel;
            set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
        }
    }
}
