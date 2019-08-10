using Newtonsoft.Json;

namespace NikitaTrubkin.StompClient.Model
{
    public class HelloMessage
    {
        [JsonProperty("name")] public string Name { get; set; }
    }
}