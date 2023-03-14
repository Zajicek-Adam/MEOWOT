using MEOWOT.Data;
using MEOWOT.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MEOWOT.Pages
{
    public class UploadTUSModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        private UserManager<IdentityUser> _userManager;

        public string UserId { get; set; }

        public List<Gallery> Galleries { get; set; }

        public UploadTUSModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGet()
        {
            UserId = _userManager.GetUserId(HttpContext.User);
            Galleries = _context.Galleries.Where(i => i.Uploader.Id == UserId).ToList();
        }
    }
}
