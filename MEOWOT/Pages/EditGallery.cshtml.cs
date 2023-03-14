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
    public class EditGalleryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        private UserManager<IdentityUser> _usermanager;
        public Gallery Gallery { get; set; }

        private IWebHostEnvironment _environment;

        [BindProperty]
        public bool Public { get; set; }

        [BindProperty]
        public Guid GalleryId { get; set; }

        [BindProperty]
        [MaxLength(32)]
        public string GalleryTitle { get; set; }


        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public EditGalleryModel(ApplicationDbContext context, UserManager<IdentityUser> usermanager, IWebHostEnvironment environment)
        {
            _context = context;
            _usermanager = usermanager;
            _environment = environment;
        }

        public IActionResult OnGet(Guid id)
        {
            Gallery = _context.Galleries.Include(i => i.Images).Include(u => u.Uploader).First(j => j.Id == id);
            if(Gallery.GetType() == typeof(DefaultGallery))
            {
                ErrorMessage = "Bruh";
                return RedirectToPage("/UserGalleries");
            }
            string UserId = _usermanager.GetUserId(HttpContext.User);
            GalleryId = id;
            Public = Gallery.Public;
            GalleryTitle = Gallery.GalleryName;

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            if (Gallery.Uploader.Id != userId)
            {
                ErrorMessage = "The gallery you tried to edit doesn't belong to you";
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
            if (_context.Galleries.Include(i => i.Uploader).First(i => i.Id == Guid.Parse(id)).Uploader.Id != userId)
            {
                ErrorMessage = "The gallery you tried to delete doesn't belong to you";
                return RedirectToPage("/UserGalleries");
            }
            Gallery = _context.Galleries.Include(i => i.Images).First(j => j.Id == Guid.Parse(id));
            if(Gallery.Images == null || Gallery.Images.Count == 0)
            {
                _context.Galleries.Remove(Gallery);
                await _context.SaveChangesAsync();
                SuccessMessage = "Gallery deleted";
                return RedirectToPage("/UserGalleries");
            }

            for (int i = 0; i < Gallery.Images.Count; i++)
            {
                var fullname = Path.Combine(_environment.ContentRootPath, "Uploads", Gallery.Images.ElementAt(i).Id.ToString().Replace("-", string.Empty));
                var fullnameEmpty = Path.Combine(_environment.ContentRootPath, "Uploads", Gallery.Images.ElementAt(i).Id.ToString());

                if (System.IO.File.Exists(fullname))
                {
                    var fileRecord = _context.Files.Find(Gallery.Images.ElementAt(i).Id);
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
                    }

                }
            }
            _context.Galleries.Remove(Gallery);
            await _context.SaveChangesAsync();
            SuccessMessage = "Gallery and its content successfully deleted";

            return RedirectToPage("/UserGalleries");

        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();
            _context.Galleries.Find(GalleryId).Public = Public;
            _context.Galleries.Find(GalleryId).GalleryName = GalleryTitle;
            if (Public)
            {
                foreach (var item in _context.Galleries.Include(l => l.Images).First(i => i.Id == GalleryId).Images)
                {
                    item.Public = true;
                    item.Gallery = _context.Galleries.Find(GalleryId);
                }
            }
            _context.SaveChanges();
            return RedirectToPage("/GalleryPhotos", new { id = GalleryId });
        }
    }
}
