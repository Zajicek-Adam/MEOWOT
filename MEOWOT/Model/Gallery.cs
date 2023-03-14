using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MEOWOT.Model
{
    public class Gallery
    {
        [Key]
        public Guid Id { get; set; }
        public IdentityUser Uploader { get; set; }

        public string GalleryName { get; set; }

        public ICollection<StoredFile> Images { get; set; }

        public bool Public { get; set; }
    }
    public class GalleryVM
    {
        [Required]
        public string GalleryName { get; set; }

        public bool Public { get; set; }

    }
}
