using MEOWOT.Data;
using MEOWOT.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using tusdotnet.Interfaces;

namespace MEOWOT.Services
{
    public class FileManager
    {
        private readonly ILogger<FileManager> _logger;
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _environment;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        private int _squareSize = 355;
        private int _sameAspectRatioHeigth = 2000;

        public FileManager(ILogger<FileManager> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }
        public async Task StoreTus(ITusFile file, CancellationToken cancellationToken)
        {

            Dictionary<string, tusdotnet.Models.Metadata> metadata = await file.GetMetadataAsync(cancellationToken); //public/private atd
            //všechny proměnný modelu
            string? filename = metadata.FirstOrDefault(m => m.Key == "filename").Value.GetString(System.Text.Encoding.UTF8);
            string? filetype = metadata.FirstOrDefault(m => m.Key == "filetype").Value.GetString(System.Text.Encoding.UTF8);
            bool isPublic = metadata.FirstOrDefault(m => m.Key == "ispublic").Value.GetString(System.Text.Encoding.UTF8) == "1" ? true : false;
            string? galleryid = metadata.FirstOrDefault(m => m.Key == "gallery").Value.GetString(System.Text.Encoding.UTF8);
            string? userid = metadata.FirstOrDefault(m => m.Key == "userid").Value.GetString(System.Text.Encoding.UTF8);
            var gallery = _context.Galleries.Include(f => f.Images).Include(k => k.Uploader).Where(c => c.Id.ToString() == galleryid).SingleOrDefault();

            var f = new StoredFile
            {
                Id = Guid.Parse(file.Id),
                Title = "",
                DateTaken = DateTime.Now,
                OriginalName = filename,
                UploadedAt = DateTime.Now,
                ContentType = filetype,
                Public = isPublic,
                Path = "",
                Uploader = _context.Users.FirstOrDefault(i => i.Id == userid),
                Gallery = gallery,
                Thumbnails = new List<Thumbnail>()
            };
            if(f.Gallery.Public == true)
            {
                f.Public = true;
            }
            //await CreateAsync(new StoredFile { StoredFileId = file.Id, OriginalName = filename, Uploaded = DateTime.Now, ContentType = filetype });
            using Stream content = await file.GetContentAsync(cancellationToken);
            content.Seek(0, SeekOrigin.Begin);

            MemoryStream ims = new MemoryStream();
            MemoryStream oms1 = new MemoryStream();
            MemoryStream oms2 = new MemoryStream();
            IImageFormat format;
            content.CopyTo(ims);
            Image img = Image.Load(ims.ToArray(), out IImageFormat form);

            var result = img.Metadata;
            DateTime imageDatetime;
            if (result.ExifProfile == null ||
                result.ExifProfile.GetValue(ExifTag.DateTimeOriginal) == null ||
                result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value == null ||
                DateTime.TryParseExact(result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value, "yyyy:MM:dd HH:mm:ss",
                                       CultureInfo.InvariantCulture, DateTimeStyles.None, out imageDatetime) == false)
            {
                f.DateTaken = f.UploadedAt; 
            }
            else
            {
                f.DateTaken = DateTime.ParseExact(result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            using (Image image = Image.Load(ims.ToArray(), out format))
            {
                int largestSize = Math.Max(image.Height, image.Width);
                if (image.Width > image.Height)
                {
                    image.Mutate(x => x.Resize(0, _squareSize));
                }
                else
                {
                    image.Mutate(x => x.Resize(_squareSize, 0));
                }
                image.Mutate(x => x.Crop(new Rectangle((image.Width - _squareSize) / 2, (image.Height - _squareSize) / 2, _squareSize, _squareSize)));
                image.Save(oms1, format);
            }
            using (Image image = Image.Load(ims.ToArray(), out format))
            {
                image.Mutate(x => x.Resize(0, _sameAspectRatioHeigth));
                image.Save(oms2, format);
            }
            f.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.Square, Blob = oms1.ToArray() });
            f.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.SameAspectRatio, Blob = oms2.ToArray() });
            _context.Files.Add(f);
            await _context.SaveChangesAsync();
            var f1 = Path.Combine(_environment.ContentRootPath, "Uploads", f.Id.ToString());
            f.Path = f1;
            using (var fileStream = new FileStream(f1, FileMode.Create))
            {
                await content.CopyToAsync(fileStream);
                await _context.SaveChangesAsync();
            };
            await _context.SaveChangesAsync();

        }
    }
}
    