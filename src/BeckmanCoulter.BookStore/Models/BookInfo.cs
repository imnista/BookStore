using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeckmanCoulter.BookStore.Models
{
    public class BookInfo
    {
        public string Author { get; set; }

        public string Publisher { get; set; }

        public string PublishData { get; set; }

        public int TotalPages { get; set; }

        public double Price { get; set; }

        public string ISBN { get; set; }
    }
}
