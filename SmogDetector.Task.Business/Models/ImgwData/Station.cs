using Newtonsoft.Json;

namespace SmogDetector.Task.Business.Models.ImgwData
{
    public class Station
    {
        [JsonProperty("kod")]
        public string Code { get; set; }

        [JsonProperty("nazwa")]
        public string Name { get; set; }
    }
}
