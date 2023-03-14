using MEOWOT.Data;
using MEOWOT.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MEOWOT.Pages
{
    [Authorize]
    public class EditImageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        private UserManager<IdentityUser> _usermanager;
        public StoredFile File { get; set; }

        private IWebHostEnvironment _environment;

        [BindProperty]
        public bool Public { get; set; }

        public List<Gallery> Galleries { get; set; }

        [BindProperty]
        public Guid FormGalleryId { get; set; }

        [BindProperty]
        public Guid FileId { get; set; }

        [BindProperty]
        [MaxLength(16)]
        public string FileTitle { get; set; }


        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public EditImageModel(ApplicationDbContext context, UserManager<IdentityUser> usermanager, IWebHostEnvironment environment)
        {
            _context = context;
            _usermanager = usermanager;
            _environment = environment;
        }

        public IActionResult OnGet(Guid id)
        {
            File = _context.Files.Include(i => i.Gallery).Include(u => u.Uploader).First(j => j.Id == id);
            string UserId = _usermanager.GetUserId(HttpContext.User);
            Galleries = _context.Galleries.AsNoTracking().Where(e => e.Uploader.Id == UserId).Include(i => i.Images).ToList();
            FileId = id;
            Public = File.Public;
            FileTitle = File.Title;

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            if (File.Uploader.Id != userId)
            {
                ErrorMessage = "The image you tried to edit doesn't belong to you";
                return RedirectToPage("/UserGalleries");
            }
            return Page();
        }
        public async Task<IActionResult> OnGetDelete(string id)
        {
            if (id == null || _context.Files == null)
            {
                return NotFound();
            }
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            if (_context.Files.Include(i => i.Uploader).First(i => i.Id == Guid.Parse(id)).Uploader.Id != userId)
            {
                ErrorMessage = "The image you tried to delete doesn't belong to you";
                return RedirectToPage("/UserGalleries");
            }
            var fullname = Path.Combine(_environment.ContentRootPath, "Uploads", id.Replace("-", string.Empty));
            var fullnameEmpty = Path.Combine(_environment.ContentRootPath, "Uploads", id);

            if (System.IO.File.Exists(fullname))
            {
                var fileRecord = _context.Files.Find(Guid.Parse(id));
                Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA " + fileRecord.OriginalName);
                if (fileRecord != null)
                {
                    _context.Files.Remove(fileRecord);
                    System.IO.File.Delete(fullname);
                    System.IO.File.Delete(fullname + ".chunkcomplete");
                    System.IO.File.Delete(fullname + ".chunkstart");
                    System.IO.File.Delete(fullname + ".metadata");
                    System.IO.File.Delete(fullname + ".uploadlength");
                    System.IO.File.Delete(fullnameEmpty);
                    SuccessMessage = "File successfully deleted";
                    await _context.SaveChangesAsync();
                }

            }

            return RedirectToPage("/UserGalleries");

        }
        public IActionResult OnPost()
        {
            _context.Files.Find(FileId).Gallery = _context.Galleries.Find(FormGalleryId);

            if (_context.Files.Find(FileId).Gallery.Public == true)
                Public = true;

            _context.Files.Find(FileId).Public = Public;
            if (String.IsNullOrEmpty(FileTitle))
            {
                _context.Files.Find(FileId).Title = "";
            }
            else
            {
                _context.Files.Find(FileId).Title = FileTitle;
            }

            _context.SaveChanges();
            return RedirectToPage("/GalleryPhotos", new { id = FormGalleryId });
        }
    }
}
