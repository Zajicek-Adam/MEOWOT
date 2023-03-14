using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MEOWOT.Model
{
    public class Thumbnail
    {
        [ForeignKey("FileId")]
        public StoredFile File { get; set; }
        [Key]
        public Guid FileId { get; set; }
        [Key]
        public ThumbnailType Type { get; set; }
        public byte[] Blob { get; set; }
    }
public enum ThumbnailType
    {
        Square,
        SameAspectRatio
    }
}
