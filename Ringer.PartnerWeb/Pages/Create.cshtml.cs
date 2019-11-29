using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.PartnerWeb.Data;

namespace Ringer.PartnerWeb.Pages
{
    public class CreateModel : PageModel
    {
        private readonly PartnerContext _context;

        public CreateModel(PartnerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public User RingerUser { get; set; }
        [BindProperty]
        public GenderType Gender { get; set; }
        [BindProperty]
        public string BirthDateString { get; set; }

        public GenderType[] GenderTypes = { GenderType.Female, GenderType.Male };

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            RingerUser.Gender = Gender;
            RingerUser.BirthDate = DateTime.Parse(BirthDateString);

            _context.Users.Add(RingerUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
