namespace ProductFilterAPI.Models
{
    public class ResultResponse
    {
        public List<Product> FilteredProducts { get; set; }
        public Filter Filter { get; set; }

    }

    public class Filter
    {
        public Decimal? MinPrice { get; set; }
        public Decimal? MaxPrice { get; set; }
        public String[] Sizes { get; set; }
        public String[] CommonWords { get; set; }
    }
}
