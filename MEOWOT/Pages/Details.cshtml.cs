using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MEOWOT.Data;
using MEOWOT.Model;

namespace MEOWOT.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public StoredFile StoredFile { get; set; }

        public async Task<IActionResult> OnGetDelete(string id)
        {
            if (id == null || _context.Files == null)
            {
                return NotFound();
            }

            var storedfile = await _context.Files.FirstOrDefaultAsync(m => m.Id == Guid.Parse(id));
            if (storedfile == null)
            {
                return NotFound();
            }

            _context.Remove(storedfile);
            await _context.SaveChangesAsync();

            System.IO.File.Delete(storedfile.Path);

            return RedirectToPage("./UserGalleries");

        }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || _context.Files == null)
            {
                return NotFound();
            }

            var storedfile = await _context.Files.FirstOrDefaultAsync(m => m.Id == Guid.Parse(id));
            if (storedfile == null)
            {
                return NotFound();
            }
            else
            {
                StoredFile = storedfile;
            }
            return Page();
        }
    }
}

