# About

[Azure Search](https://azure.microsoft.com/en-us/services/search/) is a cloud search service for web and mobile app development. This project helps you get content from a website into an Azure Search index. It uses [Abot](https://github.com/sjdirect/abot) to crawl websites. For each page it extracts the content in a customizable way and uploads the file in Azure Storage (Blob) and updates metadata in Azure SQL. Both Azure Blob and SQL get indexed by Azure Search which can be used to search the contents in the website

This project is intended as a demo or a starting point for a real crawler. At a minimum, you'll want to replace the console messages with proper logging, and customize the text extraction to improve results for your use case.


# Howto: quick start

- Create `Azure SQL`, `Azure Storage` and execute  `search-configure.ps1` to configure `Azure Search` 
- Compile the solution and execute as below

```
AzureSearchCrawler.exe -r "http://www.ganshani.com" -m 100000 -n "StorageAccountName" -k "StorageAccountKey" -s "sqlConnectionString"
```

# Howto: customize it for your project

## CrawlerConfig

The Abot crawler is configured by the method Crawler.CreateCrawlConfiguration, which you can adjust to your liking.

# Code overview

- CrawlerMain contains the setup information for `Azure Storage`, `Azure SQL`, and the main method that runs the crawler.
- The Crawler class uses Abot to crawl the given website, based off of the Abot sample. It uses a passed-in `WebPageHandler` to process each page it finds.
- `WebPageHandler` uploads the page content to Blob (`UploadToBlob`) and inserts metadata to SQL (`UpdateSql`)
- `Azure Search` (outside of this console application) scans Blob and SQL and creates single index which can be used to search 