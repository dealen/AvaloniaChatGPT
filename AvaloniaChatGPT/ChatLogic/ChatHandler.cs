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
        private OpenAIAPI? _openApi;
        private Conversation? _chat;

        public ChatHandler(OpenAIAPI api)
        {
            _openApi = api;
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

            await Task.CompletedTask;
        }

        public async Task<string> AskQuestion(string message, List<string> sentences = null)
        {
            IsChatInitialized();

            if (sentences != null)
            {
                _chat.AppendUserInput(string.Join("\n", sentences.ToArray()));
            }
            _chat.AppendUserInput(message);

            return await _chat.GetResponseFromChatbotAsync();
            // TODO

            /*
             * 
             * Error at chat/completions (https://api.openai.com/v1/chat/completions) with HTTP status code: BadRequest. Content: {
      "error": {
        "message": "This model's maximum context length is 8192 tokens. However, your messages resulted in 8202 tokens. Please reduce the length of the messages.",
        "type": "invalid_request_error",
        "param": "messages",
        "code": "context_length_exceeded"
      }
    }

            I need to handle that by moving the Token window
             */
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

        internal void Moderate(string chatMessage)
        {
            // TODO add logic here
        }
    }
}
