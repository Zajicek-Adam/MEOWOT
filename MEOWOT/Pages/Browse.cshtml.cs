using MEOWOT.Data;
using MEOWOT.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Security.Claims;

namespace MEOWOT.Pages
{
    public class BrowseModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<BrowseModel> _logger;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public List<StoredFileListViewModel> Files { get; set; } = new List<StoredFileListViewModel>();

        public List<Gallery> Galleries { get; set; }


        public BrowseModel(ILogger<BrowseModel> logger, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            Files = _context.Files
              .AsNoTracking()
              .Include(f => f.Uploader)
              .Include(f => f.Thumbnails)
              .Select(f => new StoredFileListViewModel
              {
                  Id = f.Id,
                  ContentType = f.ContentType,
                  OriginalName = f.OriginalName,
                  Uploader = f.Uploader,
                  UploadedAt = f.UploadedAt,
                  ThumbnailCount = f.Thumbnails.Count,
                  Title = f.Title,
                  Public = f.Public,
              })
              .ToList();

            Galleries = _context.Galleries
                .AsNoTracking()
                .Include(g => g.Uploader)
                .Include(i => i.Images)
                .ToList();
        }

        public IActionResult OnGetDownload(string filename)
        {
            var fullname = Path.Combine(_environment.ContentRootPath, "Uploads", filename.Replace("-", string.Empty));
            if (System.IO.File.Exists(fullname))
            {
                var filerecord = _context.Files.Find(Guid.Parse(filename));
                if (filerecord != null)
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
        public async Task<IActionResult> OnGetThumbnail(string filename, ThumbnailType type = ThumbnailType.Square)
        {
            StoredFile file = await _context.Files
              .AsNoTracking()
              .Where(f => f.Id == Guid.Parse(filename))
              .SingleOrDefaultAsync();
            if (file == null)
            {
                return NotFound("no record for this file");
            }
            Thumbnail thumbnail = await _context.Thumbnails
              .AsNoTracking()
              .Where(t => t.FileId == Guid.Parse(filename) && t.Type == type)
              .SingleOrDefaultAsync();
            if (thumbnail != null)
            {
                return File(thumbnail.Blob, file.ContentType);
            }
            return NotFound("no thumbnail for this file");
        }
        public IActionResult OnGetFile(Guid id)
        {
            var fullName = Path.Combine(_environment.ContentRootPath, "Uploads", id.ToString().Replace("-", string.Empty));
            if (System.IO.File.Exists(fullName))
            {
                var fileRecord = _context.Files.Find(id);
                if (fileRecord != null)
                {
                    Console.WriteLine("TOTO JE OG JMENO SOUBORU_: " + fileRecord.OriginalName);
                    Console.WriteLine("TOTO JE TYP SOUBORU_: " + fileRecord.ContentType);
                    Console.WriteLine("TOTO JE GANGSTA PATH SOUBORU_: " + fullName);
                    Console.WriteLine("TOTO JE MAFIANSKE ID OBRAZKU: " + id);

                    return PhysicalFile(fullName, fileRecord.ContentType, fileRecord.OriginalName);
                }
            }
            return NotFound();
        }
    }
}
