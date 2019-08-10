using Newtonsoft.Json;

namespace csharp_demo_client
{
    public class HelloMessage
    {
        [JsonProperty("name")] public string Name { get; set; }
    }
}