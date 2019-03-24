using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeckmanCoulter.BookStore.DbEntity
{
  [Table("Book")]
  public class Book
  {
    [Key]
    public int Id { get; set; }

    [Column("Name", TypeName = "nvarchar(200)")]
    public string Name { get; set; }

    public int Quantity { get; set; }

    [Column("Image", TypeName = "nvarchar(200)")]
    public string Image { get; set; }

    [Column("Description", TypeName = "nvarchar(500)")]
    public string Description { get; set; }

    [Column("UserName", TypeName = "nvarchar(256)")]
    public string UserName { get; set; }

    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
  }
}
