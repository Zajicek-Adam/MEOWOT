using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEOWOT.Model
{
    public class User : IdentityUser
    {
        [ForeignKey("DGalleryId")]
        public DefaultGallery DefGallery { get; set; }

        public Guid UserId;

        public ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();
    }
}
