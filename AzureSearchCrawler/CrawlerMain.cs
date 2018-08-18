using System;
using Fclp;

namespace AzureSearch.Crawler
{
    /// <summary>
    /// The entry point of the crawler. Adjust the constants at the top and run.
    /// </summary>
    class CrawlerMain
    {
        private const int DefaultMaxPagesToIndex = 100;

        private class Arguments
        {
            public string RootUri { get; set; }
            public int MaxPagesToIndex { get; set; }
            public string AccountKey { get; set; }
            public string AccountName { get; set; }
            public string SqlConnectionString { get; set; }
        }

        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<Arguments>();

            p.Setup(arg => arg.RootUri)
                .As('r', "rootUri")
                .Required()
                .WithDescription("Start crawling from this web page");

            p.Setup(arg => arg.MaxPagesToIndex)
                .As('m', "maxPages")
                .SetDefault(DefaultMaxPagesToIndex)
                .WithDescription("Stop after having indexed this many pages. Default is " + DefaultMaxPagesToIndex + "; 0 means no limit.");

            p.Setup(arg => arg.AccountName)
              .As('n', "StorageAccountName")
              .Required()
              .WithDescription("The name of your Azure Storage Account");

            p.Setup(arg => arg.AccountKey)
             .As('k', "StorageAccountKey")
             .Required()
             .WithDescription("The key of your Azure Storage Account");

            p.Setup(arg => arg.SqlConnectionString)
                .As('s', "SqlConnectionString")
                .Required()
                .WithDescription("Sql Connection String is required");

            p.SetupHelp("?", "h", "help").Callback(text => Console.Error.WriteLine(text));

            ICommandLineParserResult result = p.Parse(args);
            if (result.HasErrors)
            {
                Console.Error.WriteLine(result.ErrorText);
                Console.Error.WriteLine("Usage: ");
                p.HelpOption.ShowHelp(p.Options);
                return;
            }
            if (result.HelpCalled)
            {
                return;
            }

            Arguments arguments = p.Object;
            var handler = new WebPageHandler(arguments.AccountName, arguments.AccountKey, "search", arguments.SqlConnectionString);
            var crawler = new Crawler(handler);
            crawler.Crawl(arguments.RootUri, maxPages: arguments.MaxPagesToIndex).Wait();

            Console.Read(); // keep console open until a button is pressed so we see the output
        }
    }
}
