using BeckmanCoulter.BookStore.DbEntity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeckmanCoulter.BookStore.Data
{
  public class ApplicationDbContext : IdentityDbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    internal DbSet<Book> BookEntity { get; set; }

    internal DbSet<BookBorrow> BookBorrowEntity { get; set; }
  }
}
