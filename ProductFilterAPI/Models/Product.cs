using System.Text.Json.Serialization;

namespace ProductFilterAPI.Models
{
    public class Product
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("sizes")]
        public List<string> Sizes { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
