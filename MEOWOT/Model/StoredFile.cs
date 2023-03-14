using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MEOWOT.Model
{
   public class StoredFile
    {
        [Key]
        public Guid Id { get; set; } // identifikátor souboru a název fyzického souboru
        [Required]
        public IdentityUser Uploader { get; set; } // kdo soubor nahrál

        [Required]
        public DateTime UploadedAt { get; set; } // datum a čas nahrání souboru
        [Required]
        public string OriginalName { get; set; } // původní název souboru
        [Required]
        public string ContentType { get; set; } // druh obsahu v souboru (MIME type)
        public ICollection<Thumbnail> Thumbnails { get; set; } // kolekce všech možných náhledů

        public DateTime DateTaken { get; set; }

        [Required]
        public string Title { get; set; }

        public bool Public { get; set; }

        [Required]
        public string Path { get; set; }

        public Gallery Gallery { get; set; }
    }
    public class StoredFileListViewModel
    {
        public Guid Id { get; set; }
        public IdentityUser Uploader { get; set; }
        public Guid UploaderId { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime DateTaken { get; set; }
        public string OriginalName { get; set; }
        public string Title { get; set; }
        public string ContentType { get; set; }
        public int ThumbnailCount { get; set; }
        public Gallery Gallery { get; set; }
        public bool Public { get; set; }
    }
}
