using MEOWOT.Data;
using MEOWOT.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MEOWOT.Pages
{
    [Authorize]
    public class UserGalleriesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _environment;
        private readonly ILogger<UserGalleriesModel> _logger;
        private UserManager<IdentityUser> _usermanager;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public string UserId { get; set; }
        public List<StoredFileListViewModel> Files { get; set; } = new List<StoredFileListViewModel>();
        public List<Gallery> Galleries { get; set; } = new List<Gallery>();
        [BindProperty]
        public GalleryVM FormGallery { get; set; }

        public UserGalleriesModel(ILogger<UserGalleriesModel> logger, IWebHostEnvironment environment, ApplicationDbContext context, UserManager<IdentityUser> usermanager)
        {
            _usermanager = usermanager;
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public void OnGet()
        {
            UserId = _usermanager.GetUserId(HttpContext.User);
            Galleries = _context.Galleries.AsNoTracking().Where(e => e.Uploader.Id == UserId).Include(i => i.Images).ToList();

            Files = _context.Files.AsNoTracking().Include(f => f.Uploader).Include(f => f.Thumbnails).Select(f => new StoredFileListViewModel
            {
                Id = f.Id,
                ContentType = f.ContentType,
                OriginalName = f.OriginalName,
                Uploader = f.Uploader,
                UploadedAt = f.UploadedAt,
                ThumbnailCount = f.Thumbnails.Count
            })
              .ToList();
        }
        public async Task<IActionResult> OnGetThumbnail(string filename, ThumbnailType type = ThumbnailType.Square)
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
            filename = Path.GetFileName(filename);
            var fullName = Path.Combine(_environment.ContentRootPath, "Uploads", filename);
            if (System.IO.File.Exists(fullName)) // existuje soubor na disku?
            {
                var fileRecord = _context.Files.Find(Guid.Parse(filename));
                if (fileRecord != null) // je soubor v databázi?
                {
                    return PhysicalFile(fullName, fileRecord.ContentType, fileRecord.OriginalName);
                    // vra ho zpátky pod pùvodním názvem a typem
                }
                else
                {
                    ErrorMessage = "There is no record of such file.";
                    return RedirectToPage();
                }
            }
            else
            {
                ErrorMessage = "There is no such file.";
                return RedirectToPage();
            }
        }
        public async Task<IActionResult> OnPost()
        {

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value; // získáme id přihlášeného uživatele
            var user = await _usermanager.GetUserAsync(User);
            Gallery CreateGallery = new Gallery { GalleryName = FormGallery.GalleryName, Images = new List<StoredFile>(), Public = FormGallery.Public, Uploader = user, Id = Guid.NewGuid() };
            if(CreateGallery.GalleryName == null)
            {
                CreateGallery.GalleryName = "New Gallery";
            }
            await _context.Galleries.AddAsync(CreateGallery);
            await _context.SaveChangesAsync();
            return RedirectToPage("UserGalleries");

            return Page();
        }
    }
}
