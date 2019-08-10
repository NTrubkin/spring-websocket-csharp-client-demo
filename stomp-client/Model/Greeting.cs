using Newtonsoft.Json;

namespace NikitaTrubkin.StompClient.Model
{
    public class Greeting
    {
        [JsonProperty("content")] public string Content { get; set; }
    }
}