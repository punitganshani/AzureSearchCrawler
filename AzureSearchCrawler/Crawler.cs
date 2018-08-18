using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Abot.Crawler;
using Abot.Poco;

namespace AzureSearch.Crawler
{
    /// <summary>
    ///  A convenience wrapper for an Abot crawler with a reasonable default configuration and console logging.
    ///  The actual action to be performed on the crawled pages is passed in as a CrawlHandler.
    /// </summary>
    class Crawler
    {
        private static int PageCount = 0;

        private ICrawlHandler _handler;

        public Crawler(ICrawlHandler handler)
        {
            _handler = handler;
        }

        public async Task Crawl(string rootUri, int maxPages)
        {
            PoliteWebCrawler crawler = new PoliteWebCrawler(CreateCrawlConfiguration(maxPages), null, null, null, null, null, null, null, null);

            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

            CrawlResult result = crawler.Crawl(new Uri(rootUri)); //This is synchronous, it will not go to the next line until the crawl has completed
            if (result.ErrorOccurred)
            {
                Console.WriteLine("Crawl of {0} ({1} pages) completed with error: {2}", result.RootUri.AbsoluteUri, PageCount, result.ErrorException.Message);
            }
            else
            {
                Console.WriteLine("Crawl of {0} ({1} pages) completed without error.", result.RootUri.AbsoluteUri, PageCount);
            }

        }

        void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            Interlocked.Increment(ref PageCount);

            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("{0}  found on  {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        async void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            try
            {
                CrawledPage crawledPage = e.CrawledPage;
                string uri = crawledPage.Uri.AbsoluteUri;

                if (crawledPage.WebException != null || crawledPage.HttpWebResponse?.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Crawl of page failed {0}: exception '{1}', response status {2}", uri, crawledPage.WebException?.Message, crawledPage.HttpWebResponse?.StatusCode);
                    return;
                }

                if (e.CrawledPage.Uri.IsFile || !string.IsNullOrEmpty(crawledPage.Content.Text))
                {
                    await _handler.PageCrawledAsync(crawledPage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private CrawlConfiguration CreateCrawlConfiguration(int maxPages)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            CrawlConfiguration crawlConfig = new CrawlConfiguration();
            crawlConfig.CrawlTimeoutSeconds = maxPages * 10;
            crawlConfig.MaxConcurrentThreads = 5;
            crawlConfig.MinCrawlDelayPerDomainMilliSeconds = 100;
            crawlConfig.IsSslCertificateValidationEnabled = true;
            //crawlConfig.DownloadableContentTypes = "text/html, text/plain, application/pdf, application/msword, ";
            crawlConfig.MaxPageSizeInBytes = 10000000; // 100 MB
            crawlConfig.MaxPagesToCrawl = maxPages;

            return crawlConfig;
        }
    }
}
