using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using BeckmanCoulter.BookStore.Models;

namespace BeckmanCoulter.BookStore.Helper
{
    public class BookInfoHelper
    {
        private const string UrlTemplate = @"https://book.douban.com/subject_search?search_text={0}";

        private ApiHelper apiHelper = new ApiHelper();

        public async Task<bool> GetBookInfo(string name)
        {
            BookInfo bi = new BookInfo();

            //     string url = string.Format(UrlTemplate, name);
            //     string result = await apiHelper.GetHttpMethodAsync(url,null);
            //web
            //XmlDocument xDoc = new XmlDocument();
            //xDoc.InnerText = result;
            return true;
        }
    }
}
