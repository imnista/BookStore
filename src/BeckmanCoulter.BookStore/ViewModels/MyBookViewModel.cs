using System;

namespace BeckmanCoulter.BookStore.ViewModels
{
  public class MyBookViewModel
  {
    public int BookId { get; set; }
    public int BookBorrowId { get; set; }
    public string BookName { get; set; }
    public DateTime BorrowTime { get; set; }
    public DateTime ReturnTime { get; set; }
  }
}
