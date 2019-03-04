using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace BeckmanCoulter.BookStore.ViewModels
{
  public class BookViewModels
  {
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
    public IFormFile Files { get; set; }
  }
}
