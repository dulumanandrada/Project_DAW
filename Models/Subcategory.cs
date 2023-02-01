using System;
using System.ComponentModel.DataAnnotations;

namespace OurApp.Models
{
    public class Subcategory
    {
        //id = primary key
        [Key]
        public int Id { get; set; }

        //nume categorie + restrictii
        [Required(ErrorMessage = "Numele subcategoriei este obligatoriu!")]
        public string SubcategoryName { get; set; }

        public virtual ICollection<Article> Articles { get; set; }
    }
}

