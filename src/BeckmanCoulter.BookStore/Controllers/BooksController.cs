using BeckmanCoulter.BookStore.Data;
using BeckmanCoulter.BookStore.DbEntity;
using BeckmanCoulter.BookStore.Helper;
using BeckmanCoulter.BookStore.Mail;
using BeckmanCoulter.BookStore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BeckmanCoulter.BookStore.Controllers
{
  public class BooksController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly IHostingEnvironment _env;
    private readonly IMailQueueService _mailQueueService;
    private readonly ILogger<BooksController> _logger;

    private const string BookImagePath = "/bookfiles/";
    private const int PageCount = 10;

    public BooksController(ApplicationDbContext context, IHostingEnvironment env, IMailQueueService mailQueueService, ILogger<BooksController> logger)
    {
      _context = context;
      _env = env;
      _mailQueueService = mailQueueService;
      _logger = logger;
    }

    #region Index

    public IActionResult Index(int pageIndex = 1)
    {
      var bookList = _context.BookEntity.AsQueryable();
      foreach (var item in bookList)
      {
        item.Image = BookImagePath + item.Image;
      }
      ViewBag.PageCount = (bookList.Count() + PageCount - 1) / PageCount;
      ViewBag.BookListCount = bookList.Count();

      if (pageIndex == 1)
      {
        bookList = bookList.Take(PageCount);
        return View(bookList);
      }

      bookList = bookList.Skip((pageIndex - 1) * PageCount).Take(PageCount);
      return View(bookList);
    }

    #endregion

    #region Detail

    public async Task<IActionResult> Details(int id)
    {
      var book = await _context.BookEntity
        .FirstOrDefaultAsync(m => m.Id == id);

      if (book == null)
      {
        _logger.LogCritical($"Cannot get book on detail page. Id: {id}");
        return NotFound();
      }

      book.Image = BookImagePath + book.Image;
      return View(book);
    }

    /// <summary>
    /// Borrow book
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Details")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(Book model)
    {
      var book = await _context.BookEntity.Where(c => c.Id == model.Id).FirstOrDefaultAsync();

      if (book != null)
      {
        using (var transaction = _context.Database.BeginTransaction())
        {
          try
          {
            // add borrow rows
            var bookBorrow = new BookBorrow
            {
              BookId = book.Id,
              Email = Environment.UserName,
              BorrowTime = DateTime.Now
            };

            _context.Add(bookBorrow);
            await _context.SaveChangesAsync();

            // remove book quantity
            book.Quantity = book.Quantity - 1;
            _context.Update(book);
            await _context.SaveChangesAsync();

            transaction.Commit();
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Borrow book errors.");
          }
        }
      }
      else
      {
        _logger.LogCritical($"Cannot get book on borrow submit. Model: {model}");
      }

      return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Create

    public IActionResult Create()
    {
      var from = new EmailAddress("lfu01@beckman.com", "Lynn");
      var subject = "Sending with SendGrid is Fun";
      var to = new EmailAddress("lfu01@beckman.com", "Lynn");
      var plainTextContent = "and easy to do anywhere, even with C#";
      var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
      var mail = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
      _mailQueueService.SendMessage(mail);
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookViewModels bookViewModels)
    {
      if (!ModelState.IsValid)
      {
        _logger.LogError($"Create book model is invalid. Model: {bookViewModels}");
        return RedirectToAction(nameof(Index));
      }

      try
      {
        var fileName = await ProcessBookCoverImage(bookViewModels);

        var bookEntity = new Book
        {
          UserName = Environment.UserName,
          CreateTime = DateTime.Now,
          UpdateTime = DateTime.Now,
          Description = bookViewModels.Description,
          Name = bookViewModels.Name,
          Quantity = bookViewModels.Quantity,
          Image = fileName
        };

        _context.Add(bookEntity);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Create book errors.");
      }

      return RedirectToAction(nameof(Index));
    }

    private async Task<string> ProcessBookCoverImage(BookViewModels bookViewModels)
    {
      if (null == bookViewModels.Files)
      {
        return string.Empty;
      }
      else
      {
        var filePath = _env.WebRootPath + BookImagePath;
        var fileName = Guid.NewGuid() + Path.GetExtension(bookViewModels.Files.FileName);

        if (bookViewModels.Files.Length > 0)
        {
          using (var stream = new FileStream(filePath + fileName, FileMode.Create))
          {
            await bookViewModels.Files.CopyToAsync(stream);
          }
        }

        return fileName;
      }
    }

    #endregion

    #region Edit

    public async Task<IActionResult> Edit(int id)
    {
      var book = await _context.BookEntity.FindAsync(id);
      if (book == null)
      {
        _logger.LogCritical($"Cannot get book on edit page. Id: {id}");
        return NotFound();
      }
      return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Quantity,Image,Description,UserName")] Book book)
    {
      if (id != book.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          book.UserName = Environment.UserName;
          book.UpdateTime = DateTime.Now;

          _context.Update(book);
          await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, $"Edit book errors. Model: {book}");
        }
      }

      return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete

    public async Task<IActionResult> Delete(int id)
    {
      var book = await _context.BookEntity
          .FirstOrDefaultAsync(m => m.Id == id);
      if (book == null)
      {
        _logger.LogCritical($"Cannot get book on delete page. Id: {id}");
        return NotFound();
      }

      book.Image = BookImagePath + book.Image;
      return View(book);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var book = await _context.BookEntity.FindAsync(id);
      if (book == null)
      {
        _logger.LogCritical($"Cannot get book on delete confirm. Id: {id}");
        return NotFound();
      }

      var imgPath = _env.WebRootPath + BookImagePath + book.Image;
      if (System.IO.File.Exists(imgPath))
        System.IO.File.Delete(imgPath);

      _context.BookEntity.Remove(book);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Upload

    public IActionResult UploadList()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadList(List<IFormFile> files)
    {
      if (files == null || files.Count == 0)
        return RedirectToAction(nameof(Index));

      try
      {
        var file = files[0];

        if (!file.FileName.EndsWith(".xlsx"))
        {
          _logger.LogWarning($"UploadList extension is not xlsx. File name: {file.FileName}");
          return RedirectToAction(nameof(Index));
        }

        DataTable dt;
        using (var s = file.OpenReadStream())
        {
          dt = NPOIHelper.ReadStreamToDataTable(s);
        }

        var rowCount = dt.Rows.Count;
        for (var i = 0; i < rowCount; i++)
        {
          var bookEntity = new Book
          {
            UserName = dt.Rows[i]["UserName"].ToString(),
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Description = dt.Rows[i]["Description"].ToString(),
            Name = dt.Rows[i]["Name"].ToString(),
            Quantity = int.Parse(dt.Rows[i]["Quantity"].ToString()),
            Image = string.Empty,
          };

          _context.Add(bookEntity);
        }
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "UploadList errors.");
      }

      return RedirectToAction(nameof(Index));
    }

    #endregion
  }
}
