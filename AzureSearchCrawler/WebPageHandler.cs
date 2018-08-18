using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abot.Poco;
using AzureSearchCrawler.Database;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureSearch.Crawler
{
    public class WebPageHandler : ICrawlHandler
    {
        CloudBlobContainer container;

        private Dictionary<string, string> mimeTypes;

        public WebPageHandler(string storageAccountName, string storageKey, string containerName,
            string sqlConnectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageKey};EndpointSuffix=core.windows.net");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            container = blobClient.GetContainerReference(containerName);
            if (!container.Exists())
            {
                container.CreateIfNotExists();
                container.SetPermissions(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Container
                });
            }

            mimeTypes = new Dictionary<string, string>
            {
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                {".pdf", "application/pdf" },
                {".htm", "text/html" },
                {".html", "text/html" },
                {".aspx", "text/html" },
                {".asp", "text/html" }
            };
            SqlConnectionString = sqlConnectionString;
        }

        public Task CrawlFinishedAsync()
        {
            return Task.FromResult(0);
        }

        public async Task PageCrawledAsync(CrawledPage crawledPage)
        {
            var angle = crawledPage.AngleSharpHtmlDocument;
            var webPage = new WebPage(crawledPage.Uri.AbsoluteUri)
            {
                Description = angle.Head.QuerySelector("meta[name='description']")?.GetAttribute("content"),
                Keywords = angle.Head.QuerySelector("meta[name='keywords']")?.GetAttribute("content"),
                Host = crawledPage.Uri.Host,
                Path = crawledPage.Uri.AbsolutePath,
                Query = crawledPage.Uri.Query,
                Title = crawledPage.AngleSharpHtmlDocument.Title
            };

            var breadcrump = angle.Head.QuerySelector("meta[name='breadcrumb']")?.GetAttribute("content");
            var modified = angle.Head.QuerySelector("meta[name='last-modified-date']")?.GetAttribute("content")?.ToString();
            webPage.LastModifiedDate = !string.IsNullOrEmpty(modified) ? DateTime.Parse(modified) : DateTime.MinValue;

            webPage.Title = crawledPage.AngleSharpHtmlDocument.Title;

            if (!string.IsNullOrEmpty(breadcrump))
            {
                var parts = breadcrump.Split(new[] { ',' });

                if (parts.Length == 4)
                {
                    webPage.Category = parts[0];
                    webPage.SubCategory = parts[2];
                }
                else if (parts.Length == 2)
                {
                    webPage.Category = parts[0];
                }
            }

            // Content URL
            webPage.Content = await UploadToBlob(crawledPage, webPage);

            // Update SQL database
            UpdateSql(webPage);
        }

        private void UpdateSql(WebPage webPage)
        {
            using (var context = new SqlDatabase(SqlConnectionString))
            {
                var dbRecord = context.IndexResults.FirstOrDefault(x => x.Id == webPage.Id);

                if (dbRecord != null)
                {
                    dbRecord.Description = webPage.Description;
                    dbRecord.Host = webPage.Host;

                    dbRecord.IsActive = true;
                    dbRecord.Keywords = webPage.Keywords;
                    dbRecord.LastModifiedDate = webPage.LastModifiedDate;
                    dbRecord.Path = webPage.Path;
                    dbRecord.TextContent = webPage.Content;
                    dbRecord.Title = webPage.Title;
                    dbRecord.Query = webPage.Query;
                    dbRecord.Category = webPage.Category;
                    dbRecord.SubCategory = webPage.SubCategory;
                    dbRecord.Hash = webPage.Id;

                    context.Entry(dbRecord).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    var index = new IndexResult
                    {
                        Id = webPage.Id,
                        Description = webPage.Description,
                        Host = webPage.Host,
                        IsActive = true,
                        Keywords = webPage.Keywords,
                        LastModifiedDate = webPage.LastModifiedDate,
                        Path = webPage.Path,
                        TextContent = webPage.Content,
                        Title = webPage.Title,
                        Url = webPage.Url,
                        Query = webPage.Query,
                        Category = webPage.Category,
                        SubCategory = webPage.SubCategory,
                        Hash = webPage.Id
                    };

                    context.IndexResults.Add(index);
                    context.SaveChanges();
                }
            }
        }

        private async Task<string> UploadToBlob(CrawledPage crawledPage, WebPage webPage)
        {
            var fileName = GetFileNameFromUrl(crawledPage.Uri.AbsoluteUri);
            var extension = Path.GetExtension(fileName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{Guid.NewGuid()}{extension}");
            await blockBlob.UploadFromByteArrayAsync(crawledPage.Content.Bytes, 0, crawledPage.Content.Bytes.Count());

            // Required for Azure Search Index
            blockBlob.Metadata.Add("mykey", webPage.Id);
            await blockBlob.SetMetadataAsync();

            if (mimeTypes.ContainsKey(extension))
            {
                blockBlob.Properties.ContentType = mimeTypes[extension];
                await blockBlob.SetPropertiesAsync();
            }
            else if (string.IsNullOrEmpty(extension))
            {
                // URLs like https://www.abc.com/
            }
            else
            {
                Console.WriteLine($">>>> No MIME found for type {extension}");
            }

            return blockBlob.Uri.ToString();
        }

        readonly static Uri SomeBaseUri = new Uri("http://canbeanything");

        public string SqlConnectionString { get; }

        static string GetFileNameFromUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return Path.GetFileName(uri.LocalPath);
        }
    }
}
