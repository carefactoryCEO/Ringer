using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.PartnerWeb.Data;

namespace Ringer.PartnerWeb.Pages
{
    public class EditModel : PageModel
    {
        private readonly PartnerContext _context;

        public EditModel(PartnerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User RingerUser { get; set; }

        [BindProperty]
        public GenderType Gender { get; set; }

        [BindProperty]
        [Required]
        public string BirthDateString
        {
            get
            {
                return RingerUser.BirthDate.ToString("yyyy-MM-dd");
            }
            set
            {
                RingerUser.BirthDate = DateTime.Parse((string)value);
            }
        }

        public GenderType[] GenderTypes = { GenderType.Female, GenderType.Male };


        public async Task<IActionResult> OnGetAsync(int id)
        {

            RingerUser = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

            if (RingerUser == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(RingerUser).State = EntityState.Modified;

            try
            {
                RingerUser.Gender = Gender;
                RingerUser.BirthDate = DateTime.Parse(BirthDateString);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(RingerUser.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
