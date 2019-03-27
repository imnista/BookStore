using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BeckmanCoulter.BookStore.ViewModels
{
  public class BookViewModels
  {
    [Required]
    public string Name { get; set; }

    [Required]
    public int Quantity { get; set; }

    public string Description { get; set; }

    public IFormFile Files { get; set; }
  }
}
