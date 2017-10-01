using Newtonsoft.Json;

namespace SmogDetector.Task.Business.Models.ImgwData
{
    public class Classification
    {
        [JsonProperty("kod")]
        public string Code { get; set; }

        [JsonProperty("nazwa")]
        public string Name { get; set; }

        [JsonProperty("jednostka")]
        public string Unit { get; set; }
    }
}
