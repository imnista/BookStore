using System;

namespace BeckmanCoulter.BookStore.ViewModels
{
  public class BookBorrowViewModel
  {
    public string BookName { get; set; }
    public string BorrowEmail { get; set; }
    public DateTime BorrowTime { get; set; }
  }
}
