namespace AzureSearchCrawler.Database
{
    public class IndexResult
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string TextContent { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public bool IsActive { get; set; }
        public string BreadCrump { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Hash { get; set; }
    }
}
