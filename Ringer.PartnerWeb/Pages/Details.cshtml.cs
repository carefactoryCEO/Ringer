using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ringer.Core.Models;
using Ringer.PartnerWeb.Data;

namespace Ringer.PartnerWeb.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly Ringer.PartnerWeb.Data.PartnerContext _context;

        public DetailsModel(Ringer.PartnerWeb.Data.PartnerContext context)
        {
            _context = context;
        }

        public User User { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            User = await _context.Users.FirstOrDefaultAsync(m => m.ID == id);

            if (User == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
