using System.Data.Entity;

namespace AzureSearchCrawler.Database
{
    public class SqlDatabase : DbContext
    {
        public SqlDatabase(string connectionString) : base(connectionString)
        {

        }

        public virtual DbSet<IndexResult> IndexResults { get; set; }
    }
}
