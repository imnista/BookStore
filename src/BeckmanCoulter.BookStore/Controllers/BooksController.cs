using BeckmanCoulter.BookStore.Data;
using BeckmanCoulter.BookStore.DbEntity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeckmanCoulter.BookStore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace BeckmanCoulter.BookStore.Controllers
{
  public class BooksController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly IHostingEnvironment _env;

    public BooksController(ApplicationDbContext context, IHostingEnvironment env)
    {
      _context = context;
      _env = env;
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
        var filePath = _env.WebRootPath + @"\bookfiles\";
        var fileName = Guid.NewGuid() + Path.GetExtension(bookViewModels.Files.FileName);

        if (bookViewModels.Files.Length > 0)
        {
          using (var stream = new FileStream(filePath + fileName, FileMode.Create))
          {
            await bookViewModels.Files.CopyToAsync(stream);
          }
        }

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
  }
}
