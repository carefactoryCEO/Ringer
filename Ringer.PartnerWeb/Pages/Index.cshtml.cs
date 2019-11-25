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
    public class IndexModel : PageModel
    {
        private readonly PartnerContext _context;

        public IndexModel(PartnerContext context)
        {
            _context = context;
        }

        public IList<User> Users { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
        }
    }
}
