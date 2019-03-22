using BeckmanCoulter.BookStore.Data;
using BeckmanCoulter.BookStore.DbEntity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeckmanCoulter.BookStore.Helper;
using BeckmanCoulter.BookStore.Mail;
using BeckmanCoulter.BookStore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SendGrid.Helpers.Mail;

namespace BeckmanCoulter.BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _env;
        private readonly IMailQueueService _mailQueueService;

        public BooksController(ApplicationDbContext context, IHostingEnvironment env, IMailQueueService mailQueueService)
        {
            _context = context;
            _env = env;
            _mailQueueService = mailQueueService;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.BookEntity.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.BookEntity
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            book.Image = _env.WebRootPath + @"\bookfiles\" + book.Image;
            return View(book);
        }

        // GET: Books/Create
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

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModels bookViewModels)
        {
            if (ModelState.IsValid)
            {
                var fileName = await ProcessBookCoverImage(bookViewModels);

                var bookEntity = new Book
                {
                    Id = Guid.NewGuid(),
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
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UploadList(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return RedirectToAction(nameof(Index));

            try
            {
                var req = this.Request;

                var file = files[0];

                if (!file.FileName.EndsWith(".xlsx"))
                    return RedirectToAction(nameof(Index));

                DataTable dt;
                using (Stream s = file.OpenReadStream())
                {
                    dt = NPOIHelper.ReadStreamToDataTable(s, null, true);
                }

                int rowCount = dt.Rows.Count;
                for (int i = 0; i < rowCount; i++)
                {
                    var bookEntity = new Book
                    {
                        Id = Guid.NewGuid(),
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
            catch (Exception e)
            {
                RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.BookEntity.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Quantity,Image,Description,UserName")] Book book)
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
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.BookEntity
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var book = await _context.BookEntity.FindAsync(id);
            _context.BookEntity.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(Guid id)
        {
            return _context.BookEntity.Any(e => e.Id == id);
        }

        private async Task<string> ProcessBookCoverImage(BookViewModels bookViewModels)
        {
            if (null == bookViewModels.Files)
            {
                return string.Empty;
            }
            else
            {
                var filePath = _env.WebRootPath + @"\bookfiles\";
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
    }
}
