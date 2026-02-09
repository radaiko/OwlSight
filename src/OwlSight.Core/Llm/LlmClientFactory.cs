using Microsoft.Extensions.AI;
using OpenAI;
using OwlSight.Core.Configuration;

namespace OwlSight.Core.Llm;

public sealed class LlmClientFactory
{
    public IChatClient Create(LlmConfig config)
    {
        var options = new OpenAIClientOptions { Endpoint = new Uri(config.BaseUrl) };
        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(config.ApiKey), options);
        return openAiClient.GetChatClient(config.Model).AsIChatClient();
    }
}
