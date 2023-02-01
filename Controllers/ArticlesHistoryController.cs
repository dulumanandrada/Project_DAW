using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OurApp.Data;
using OurApp.Models;

namespace OurApp.Controllers
{
    public class ArticlesHistoryController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ArticlesHistoryController(ApplicationDbContext context,
                                    UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var Articole = db.ArticlesHistories.OrderBy(a => a.ArticleId);

            ViewBag.ArticlesHist = Articole;

          
            if (User.IsInRole("Admin"))
            {
                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa vedeti istoricul articolelor";
                return Redirect("Articles/Index");
            }
            
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Show(int id)
        {
               var articole = db.ArticlesHistories.Include("User")
                                                .Where(articole => articole.ArticleId == id)
                                                .Where(articole => articole.UserId == _userManager.GetUserId(User))
                                                .OrderBy(a => a.Id);
                ViewBag.ArticlesHistories = articole;

            return View();

            /*
            if (articole.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(articole);
            }
            

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Articles/Index");
            }
            */
            
        }

    }
}
