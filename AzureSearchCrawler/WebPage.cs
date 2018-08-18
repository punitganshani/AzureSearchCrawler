using System;
using Microsoft.Azure.Search.Models;

namespace AzureSearch.Crawler
{
    [SerializePropertyNamesAsCamelCase]
    public class WebPage
    {
        public WebPage(string url)
        {
            Url = url;

            Id = url.GetHashCode().ToString();
        }

        public string Id { get; }
        public string Url { get; }
        public string Content { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }

    }
}
