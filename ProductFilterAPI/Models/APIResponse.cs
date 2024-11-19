using ProductFilterAPI.Controllers;
using System.Text.Json.Serialization;

namespace ProductFilterAPI.Models
{
    public class APIResponse
    {
        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }
        public ApiKeys ApiKeys { get; set; }
    }

    public class ApiKeys
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }
    }
}
