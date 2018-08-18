using System.Threading.Tasks;

using Abot.Poco;

namespace AzureSearch.Crawler
{
    /// <summary>
    /// A generic callback handler to be passed into a Crawler.
    /// </summary>
    public interface ICrawlHandler
    {
        Task PageCrawledAsync(CrawledPage page);

        Task CrawlFinishedAsync();
    }
}
