using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurApp.Data;
using OurApp.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace project2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubcategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        public SubcategoriesController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var subcategories = from subcategory in db.Subcategories
                                orderby subcategory.SubcategoryName
                                select subcategory;

            ViewBag.Subcategories = subcategories;
            return View();
        }

        public IActionResult Show(int id)
        {
            Subcategory subcategory = db.Subcategories.Find(id);
            ViewBag.Subcategory = subcategory;
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Subcategory subcat)
        {
            try
            {
                db.Subcategories.Add(subcat);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            Subcategory subcategory = db.Subcategories.Find(id);
            ViewBag.Subcategory = subcategory;
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, Subcategory requestSubcategory)
        {
            try
            {
                Subcategory subcategory = db.Subcategories.Find(id);
                {
                    subcategory.SubcategoryName = requestSubcategory.SubcategoryName;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.Subcategory = requestSubcategory;
                return View();
            }  
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Subcategory subcategory = db.Subcategories.Find(id);
            db.Subcategories.Remove(subcategory);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

