using OpenAI_API.Chat;
using OpenAI_API;
using OpenAI_API.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using MsBox.Avalonia;

namespace AvaloniaChatGPT.ChatLogic
{
    // TODO: 
    /* - add moderation 
     * - add settings
     * -- temperature
     * -- model
     * -- length (tokens)
     * -- api key
     * - system message
     * - history of conversations
     */

    internal class ChatHandler
    {
        private OpenAIAPI _openApi;
        private Conversation _chat;

        public ChatHandler(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException("Api key is mandatory!!!");
            }

            _openApi = new OpenAIAPI(apiKey);
            _chat = _openApi.Chat.CreateConversation(new ChatRequest
            {
                Model = Model.GPT4,
                Temperature = 0.8,
            });
        }

        internal async Task SetTemperature(double temperature)
        {
            if (_chat != null)
            {
                //var model = await _chat.Model.RetrieveModelDetailsAsync(_openApi);
                
            }
        }

        public async Task<string> AskQuestion(string message, List<string> sentences = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                var msg = MessageBoxManager.GetMessageBoxStandard("Missing question error", "Please fill question field.");
                await msg.ShowAsync();
                return string.Empty;
            }

            IsChatInitialized();

            if (sentences != null)
            {
                _chat.AppendUserInput(string.Join("\n", sentences.ToArray()));
            }
            _chat.AppendUserInput(message);

            return await _chat.GetResponseFromChatbotAsync();
        }

        private void IsChatInitialized()
        {
            if (_chat == null)
            {
                throw new NullReferenceException("Chat is not initialized!");
            }
        }

        public void AddSystemMessage(string systemMessage)
        {
            IsChatInitialized();
            _chat.AppendSystemMessage(systemMessage);
        }

        public void EndConversation()
        {
            _chat = null;
            _openApi = null;
        }
    }
}
