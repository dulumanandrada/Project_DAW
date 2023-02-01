using System;
using System.ComponentModel.DataAnnotations;

namespace OurApp.Models
{
    public class Category
    {
        //id = primary key
        [Key]
        public int Id { get; set; }

        //nume categorie + restrictii
        [Required(ErrorMessage = "Numele categoriei este obligatoriu!")]
        public string CategoryName { get; set; }

        public virtual ICollection<Article> Articles { get; set; }
    }
}

