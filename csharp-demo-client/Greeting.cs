using Newtonsoft.Json;

namespace csharp_demo_client
{
    public class Greeting
    {
        [JsonProperty("content")] public string Content { get; set; }
    }
}