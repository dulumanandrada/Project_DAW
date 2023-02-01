﻿using Microsoft.AspNetCore.Mvc;
using OurApp.Data;
using OurApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using static Humanizer.On;
using Ganss.Xss;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OurApp.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ArticlesController(ApplicationDbContext context,
                                    UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //se afiseaza lista tuturor articolelor
        //HttpGet implicit
        //este valabil pentru toti userii inregistrati
        [Authorize(Roles = "User,Editor,Admin")]

        public IActionResult Index()
        {
            var articles = db.Articles.Include("Category")
                                .Include("Subcategory")
                                .Include("User").OrderBy(a => a.CreateDate);
            var search = "";

            ViewBag.Articles = articles;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Msg = TempData["message"];
            }

            // MOTOR DE CAUTARE

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in articol (Title si Content)

                List<int> articleIds = db.Articles.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                // Cautare in comentarii (Content)
                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                            c => c.Content.Contains(search)
                                         ).Select(c => (int)c.ArticleId)
                                         .ToList();


                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();


                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include("Category")
                                      .Include("Subcategory")
                                      .Include("User")
                                      .OrderBy(a => a.CreateDate);

            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            // Alegem sa afisam 3 articole pe pagina
            int _perPage = 3;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }


            // Fiind un numar variabil de articole, verificam de fiecare data utilizand 
            // metoda Count()

            int totalItems = articles.Count();


            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Articles/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam 
            // in functie de offset
            var paginatedArticles = articles.Skip(offset).Take(_perPage);


            // Preluam numarul ultimei pagini

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Articles = paginatedArticles;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }


            return View();
        }

        //se afiseaza un singur articol in functie de id
        //sunt preluate toate datele sale
        //HttpGet implicit
        [Authorize(Roles = "User,Editor,Admin")]

        public IActionResult Show(int id)
        {
            Article article = db.Articles.Include("Category")
                                            .Include("Subcategory")
                                            .Include("User")
                                            .Include("Comments")
                                            .Include("Comments.User")
                                .Where(article => article.Id == id)
                                .First();

            // Adaugam bookmark-urile utilizatorului pentru dropdown
            ViewBag.UserBookmarks = db.Bookmarks
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();

            SetAccessRights();

            return View(article);
        }

        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }

            else
            {
                Article art = db.Articles.Include("Category")
                                         .Include("Subcategory")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == comment.ArticleId)
                                         .First();

                //return Redirect("/Articles/Show/" + comment.ArticleId);

                // Adaugam bookmark-urile utilizatorului pentru dropdown
                ViewBag.UserBookmarks = db.Bookmarks
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();


                SetAccessRights();

                return View(art);
            }
        }


        [HttpPost]
        public IActionResult AddBookmark([FromForm] ArticleBookmark articleBookmark)
        {
            // Daca modelul este valid
            
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja articolul in colectie
                if (db.ArticleBookmarks
                    .Where(ab => ab.ArticleId == articleBookmark.ArticleId)
                    .Where(ab => ab.BookmarkId == articleBookmark.BookmarkId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest articol este deja adaugat in colectie";
                    TempData["messageType"] = "alert-danger";
                }
                
                else
                {
                    // Adaugam asocierea intre articol si bookmark 
                    db.ArticleBookmarks.Add(articleBookmark);
                    
                    // Salvam modificarile
                    db.SaveChanges();

                    // Adaugam un mesaj de success
                    TempData["message"] = "Articolul a fost adaugat in colectia selectata";
                    TempData["messageType"] = "alert-success";
                }
                
            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga articolul in colectie";
                TempData["messageType"] = "alert-danger";
            }
            
            // Ne intoarcem la pagina articolului
            return Redirect("/Articles/Show/" + articleBookmark.ArticleId);
        }


        //se afiseaza formularul pentru adaugarea unui articol nou
        //HttpGet implicit
        //doar admin sau editor pot adauga articole

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Article article = new Article();
            article.Categ = GetAllCategories();
            article.Subcateg = GetAllSubcategories();

            return View(article);
        }

        //se adauga articolul in baza de date
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New(Article article)
        {
            var sanitizer = new HtmlSanitizer();

            article.CreateDate = DateTime.Now;
            article.LastDate = article.CreateDate;
            
            article.UserId = _userManager.GetUserId(User);
            
            if(ModelState.IsValid)
            {
                article.Content = sanitizer.Sanitize(article.Content);
                article.Content = (article.Content);

                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat!";
                return RedirectToAction("Index");
            }
            else
            {
                article.Categ = GetAllCategories();
                article.Subcateg = GetAllSubcategories();
                return View(article);
            }
            /*
            try
            {
                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat";
                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                //return RedirectToAction("New");
                return View(article);   //pastreaza elementele pe care le am completat!
            }
            */
        }

        //se editeaza un articol existent
        //se afiseaza formularul aferent articolului
        //httpget implicit
        /*
        [Authorize(Roles = "Editor,Admin")]

        public IActionResult Edit(int id)
        {
            Article article = db.Articles.Include("Category").Include("Subcategory")
                                .Where(art => art.Id == id)
                                .First();
            ViewBag.Article = article;
            ViewBag.Category = article.Category;
            ViewBag.Subcategory = article.Subcategory;

            var categories = from categ in db.Categories
                             select categ;
            var subcategories = from subcateg in db.Subcategories
                                select subcateg;

            ViewBag.Categories = categories;
            ViewBag.Subcategories = subcategories;

            if(article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View();
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol ce nu va apartine!";
                return RedirectToAction("Index");
            }
            
        }

        //se adauga articolul modificat in baza de date
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public IActionResult Edit(int id, Article requestArticle)
        {
            Article article = db.Articles.Find(id);

            try
            {
                if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    ArticlesHistory articlesHistory = new();

                    articlesHistory.ArticleId = article.Id;
                    articlesHistory.Title = article.Title;
                    articlesHistory.Content = article.Content;
                    articlesHistory.EditDate = DateTime.Now;
                    articlesHistory.Category = article.Category;
                    articlesHistory.Category = article.Category;
                    articlesHistory.Subcategory = article.Subcategory;
                    articlesHistory.CategoryId = article.CategoryId;
                    articlesHistory.SubcategoryId = article.SubcategoryId;
                    articlesHistory.UserId = _userManager.GetUserId(User);

                    db.ArticlesHistories.Add(articlesHistory);

                    article.Title = requestArticle.Title;
                    article.Content = requestArticle.Content;
                    article.LastDate = DateTime.Now;
                    article.CategoryId = requestArticle.CategoryId;
                    article.SubcategoryId = requestArticle.SubcategoryId;

                    TempData["message"] = "Articolul a fost editat!";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol ce nu va apartine!";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception)
            {
                return RedirectToAction("Edit", id);
            }
        }
        */


        //=========== asta am adaugat:
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Article article = db.Articles.Include("Category")
                                        .Include("Subcategory")
                                        .Where(art => art.Id == id)
                                        .First();

            article.Categ = GetAllCategories();
            article.Subcateg = GetAllSubcategories();

            if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(article);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }

        }

        // Se adauga articolul modificat in baza de date
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Article requestArticle)
        {
            var sanitizer = new HtmlSanitizer();

            Article article = db.Articles.Find(id);


            if (ModelState.IsValid)
            {
                if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    ArticlesHistory articlesHistory = new();

                    articlesHistory.ArticleId = article.Id;
                    articlesHistory.Title = article.Title;
                    articlesHistory.Content = article.Content;
                    articlesHistory.EditDate = DateTime.Now;
                    articlesHistory.Category = article.Category;
                    articlesHistory.Category = article.Category;
                    articlesHistory.Subcategory = article.Subcategory;
                    articlesHistory.CategoryId = article.CategoryId;
                    articlesHistory.SubcategoryId = article.SubcategoryId;
                    articlesHistory.UserId = _userManager.GetUserId(User);

                    db.ArticlesHistories.Add(articlesHistory);

                    article.Title = requestArticle.Title;

                    requestArticle.Content = sanitizer.Sanitize(requestArticle.Content);

                    article.Content = requestArticle.Content;
                    article.LastDate = DateTime.Now;
                    article.CategoryId = requestArticle.CategoryId;
                    article.SubcategoryId = requestArticle.SubcategoryId;
                    TempData["message"] = "Articolul a fost modificat";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestArticle.Categ = GetAllCategories();
                return View(requestArticle);
            }
        }
        //========= pana aici



        //se sterge un articol din baza de date
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public IActionResult Delete(int id)
        {
            Article article = db.Articles.Include("Comments")
                                          .Where(art => art.Id == id)
                                          .First();

            if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Articles.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol ce nu va apartine!";
                return RedirectToAction("Index");
            }

        }

        //functia cu lista de categorii
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            //generam o lista fara elemente
            var selectList = new List<SelectListItem>();

            //extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            //iteram prin categorii
            foreach (var category in categories)
            {
                //adaugam in lista categoriile
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }

        //functia cu lista de subcategorii
        [NonAction]
        public IEnumerable<SelectListItem> GetAllSubcategories()
        {
            //generam o lista fara elemente
            var selectList = new List<SelectListItem>();

            //extragem toate categoriile din baza de date
            var subcategories = from subcat in db.Subcategories
                             select subcat;

            //iteram prin categorii
            foreach (var subcategory in subcategories)
            {
                //adaugam in lista categoriile
                selectList.Add(new SelectListItem
                {
                    Value = subcategory.Id.ToString(),
                    Text = subcategory.SubcategoryName.ToString()
                });
            }

            return selectList;
        }


        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }



        //==============

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ShowAfterCategory(int? id)
        {
            var articles = db.Articles.Include("Category")
                                       .Include("Subcategory")
                                       .Include("User")
                                .Where(a => a.CategoryId == id);

            ViewBag.ArticlesAfterCat = articles;
            ViewBag.IdCat = id;

            //SetAccessRights();
            /*
            try
            {
                return View(articles);
            }

            catch (NullReferenceException e)
            {
                return RedirectToAction("Index");
            }
            */
            return View();

        }

        //==============
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult GoBackTo(int id)
        {
            ArticlesHistory articleh = db.ArticlesHistories.Where(a => a.Id == id).First();



            var articol = db.Articles
                                .Where(a => a.Id == articleh.ArticleId)
                                .First();

            articol.Title = articleh.Title;
            articol.Content = articleh.Content;

            db.SaveChanges();
            return RedirectToAction("Index");
        }
        //===============


        public IActionResult IndexSortAlf(int id)
        {
            var articles = db.Articles.Include("Category")
                                .Include("Subcategory")
                                .Include("User")
                                .Where(a => a.CategoryId == id)
                                .OrderBy(a => a.Title);

            ViewBag.ArticlesSortAlf = articles;
            ViewBag.IdCat = id;

            return View();
        }

        public IActionResult IndexSortDate(int id)
        {
            var articles = db.Articles.Include("Category")
                                .Include("Subcategory")
                                .Include("User")
                                .Where(a => a.CategoryId == id)
                                .OrderBy(a => a.CreateDate);

            ViewBag.ArticlesSortDate = articles;
            ViewBag.IdCat = id;

            return View();
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }
    }
}

