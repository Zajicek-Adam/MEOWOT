using MEOWOT.Data;
using MEOWOT.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MEOWOT.Pages
{
    public class GalleryPhotosModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _environment;
        private readonly ILogger<GalleryPhotosModel> _logger;
        private UserManager<IdentityUser> _usermanager;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public string UserId { get; set; }
        public List<StoredFileListViewModel> Files { get; set; } = new List<StoredFileListViewModel>();
        public Gallery Gallery { get; set; }

        public GalleryPhotosModel(ILogger<GalleryPhotosModel> logger, IWebHostEnvironment environment, ApplicationDbContext context, UserManager<IdentityUser> usermanager)
        {
            _usermanager = usermanager;
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult OnGet(Guid id)
        {
            UserId = _usermanager.GetUserId(HttpContext.User);
            Gallery = _context.Galleries.AsNoTracking().Include(u => u.Uploader).Where(e => e.Id == id).FirstOrDefault();

            string GalleryUserId = _context.Galleries.Include(k => k.Uploader).Where(j => j.Id == id).FirstOrDefault().Uploader.Id;

            if ((!Gallery.Public && !User.Identity.IsAuthenticated) || (!Gallery.Public && GalleryUserId != this.UserId))
            {
                return RedirectToPage("/Browse");
            }

            Files = _context.Files.AsNoTracking().Include(f => f.Uploader).Include(f => f.Thumbnails).Include(x => x.Gallery).Where(k => k.Gallery == Gallery).Select(f => new StoredFileListViewModel
            {
                Id = f.Id,
                ContentType = f.ContentType,
                OriginalName = f.OriginalName,
                Uploader = f.Uploader,
                UploadedAt = f.UploadedAt,
                ThumbnailCount = f.Thumbnails.Count,
                Title = f.Title,
                DateTaken = f.DateTaken,
                Gallery = f.Gallery
            })
              .ToList();
            return Page();
        }
        public async Task<IActionResult> OnGetThumbnail(string filename, ThumbnailType type = ThumbnailType.SameAspectRatio)
        {
            StoredFile file = await _context.Files.AsNoTracking().Where(f => f.Id == Guid.Parse(filename)).SingleOrDefaultAsync();
            if (file == null)
            {
                return NotFound("no record for this file");
            }

            Thumbnail thumbnail = await _context.Thumbnails.AsNoTracking().Where(t => t.FileId == Guid.Parse(filename) && t.Type == type).SingleOrDefaultAsync();
            if (thumbnail != null)
            {
                return File(thumbnail.Blob, file.ContentType);
            }
            return NotFound("no thumbnail for this file");
        }

        public IActionResult OnGetDownload(string filename)
        {
            StoredFile file = _context.Files
             .AsNoTracking()
             .Include(i => i.Uploader)
             .Where(f => f.Id == Guid.Parse(filename))
             .SingleOrDefault();

            string userid;

            if(User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault() == null && file.Public == false)
            {
                ErrorMessage = "Restricted access";
                return RedirectToPage("/Browse");
            }
            else
            {
                userid = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            }
            
            if (file.Uploader.Id != userid && file.Public == false)
            {
                ErrorMessage = "You don't have access to that image";
                return RedirectToPage("/Browse");
            }

            var fullname = Path.Combine(_environment.ContentRootPath, "Uploads", filename.Replace("-", string.Empty));
            if (System.IO.File.Exists(fullname))
            {
                var filerecord = _context.Files.Find(Guid.Parse(filename));
                if(filerecord != null)
                {
                    return PhysicalFile(fullname, filerecord.ContentType, filerecord.OriginalName);
                }
                else
                {
                    ErrorMessage = "There is no record of such file.";
                }
            }
            return Page();
        }
        public IActionResult OnGetToggle(Guid id)
        {
            Gallery = _context.Galleries.AsNoTracking().Where(e => e.Id == id).FirstOrDefault();
            try
            {
                if (Gallery.Public)
                {
                    Gallery.Public = false;
                    SuccessMessage = "Gallery has been set to private.";
                    return RedirectToPage(id);
                }
                else
                {
                    Gallery.Public = true;
                    SuccessMessage = "Gallery has been set to public";
                    return RedirectToPage(id);
                }
            }
            catch
            {
                ErrorMessage = "There has been an error.";
                return RedirectToPage(id);
            }
        }
        public async Task<IActionResult> OnGetDelete(string id)
        {
            if (id == null || _context.Files == null)
            {
                return NotFound();
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
        /*
        public async Task<IActionResult> OnGetDelete(string filename)
        {
            Guid id = Guid.Parse(filename);
            var file = await _context.Files.FindAsync(id);
            if (file != null)
            {
                _context.Files.Remove(file);
                _context.SaveChanges();
            }
            else
            {
                return NotFound("record doesn't exist");
            }

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;

            filename = Path.GetFileNameWithoutExtension(filename);
            string toDelete = filename + ".*";
            string toDeleteWith = toDelete.Replace("-", "");
            string[] files = System.IO.Directory.GetFiles(Path.Combine(_environment.ContentRootPath, "Uploads"), toDelete);
            string[] files2 = System.IO.Directory.GetFiles(Path.Combine(_environment.ContentRootPath, "Uploads"), toDeleteWith);
            string[] filesToDelete = files.Concat(files2).ToArray();
            foreach (string f in filesToDelete)
            {
                System.IO.File.Delete(f);
            }

            return RedirectToPage();
        }*/
        public async Task<IActionResult> OnGetDeleteGal(Guid id)
        {
            if (id == null || _context.Galleries == null)
            {
                return NotFound();
            }
            Gallery thisGallery = _context.Galleries.Include(i => i.Images).First(j => j.Id == id);

            if (thisGallery.Images != null)
            {
                foreach (var storedfile in thisGallery.Images)
                {
                    var fullname = Path.Combine(_environment.ContentRootPath, "Uploads", storedfile.Id.ToString().Replace("-", string.Empty));
                    var fullnameEmpty = Path.Combine(_environment.ContentRootPath, "Uploads", storedfile.Id.ToString());

                    System.IO.File.Delete(fullnameEmpty);
                    System.IO.File.Delete(fullname + ".chunkcomplete");
                    System.IO.File.Delete(fullname + ".chunkstart");
                    System.IO.File.Delete(fullname + ".metadata");
                    System.IO.File.Delete(fullname + ".uploadlength");
                    System.IO.File.Delete(fullname);
                }
            }
            foreach (var x in _context.Files.Include(i => i.Gallery))
            {
                if (x.Gallery.Id == id)
                {
                    _context.Files.Remove(x);
                }
            }

            _context.Remove(thisGallery);
            await _context.SaveChangesAsync();




            SuccessMessage = "Gallery successfully deleted";

            return RedirectToPage("/UserGalleries");

        }
    }
}
