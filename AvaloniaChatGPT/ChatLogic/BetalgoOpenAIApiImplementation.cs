using OpenAI.Managers;

namespace AvaloniaChatGPT.ChatLogic
{
    internal class BetalgoOpenAIApiImplementation
    {
        private readonly OpenAIService _openAIService;

        public BetalgoOpenAIApiImplementation(string apiKey, string modelId)
        {
            _openAIService = new OpenAIService(new OpenAI.OpenAiOptions
            {
                ApiKey = apiKey
            });
            _openAIService.SetDefaultModelId(OpenAI.ObjectModels.Models.Gpt_4_32k);
        }
    }
}
