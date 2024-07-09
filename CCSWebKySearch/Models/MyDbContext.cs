using Microsoft.EntityFrameworkCore;

namespace CCSWebKySearch.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<NotebookModel> Notebooks { get; set; }
    }
}
