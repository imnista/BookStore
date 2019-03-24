using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeckmanCoulter.BookStore.DbEntity
{
  [Table("BookBorrow")]
  public class BookBorrow
  {
    [Key]
    public int Id { get; set; }

    public int BookId { get; set; }

    [Column("Email", TypeName = "nvarchar(256)")]
    public string Email { get; set; }

    public DateTime BorrowTime { get; set; }

    public DateTime ReturnTime { get; set; }

    public DateTime DueTime { get; set; }
  }
}
