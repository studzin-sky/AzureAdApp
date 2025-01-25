using AzureAdApp.Data;
using AzureAdApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<IdentityUser> _signInManager;

    public IndexModel(ApplicationDbContext context, SignInManager<IdentityUser> signInManager)
    {
        _context = context;
        _signInManager = signInManager;
    }

    public IList<Advertisement> Advertisements { get; set; }

    public async Task OnGetAsync()
    {
        Advertisements = await _context.Advertisements
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await _signInManager.SignOutAsync(); 
        return RedirectToPage("/Account/Login"); 
    }
}
