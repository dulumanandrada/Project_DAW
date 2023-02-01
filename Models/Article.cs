using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OurApp.Models
{
    public class Article
    {
        //id = primary key
        [Key]
        public int Id { get; set; }

        //titlu + restrictiile sale (erorile)
        [Required(ErrorMessage = "Titlul este obligatoriu!")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere!")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba cel putin 5 caractere!")]
        public string Title { get; set; }

        //cotinut
        [Required(ErrorMessage = "Continutul articolului este obligatoriu!")]
        public string Content { get; set; }

        //create date
        public DateTime CreateDate { get; set; }

        //last update
        public DateTime LastDate { get; set; }

        //categoria = foreign key
        [Required(ErrorMessage = "Categoria este obligatorie!")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        //subcategoria = foreign key
        [Required(ErrorMessage = "Subcategoria este obligatorie!")]
        public int? SubcategoryId { get; set; }
        public virtual Subcategory? Subcategory { get; set; }

        //user = foreign key 
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } //un articol are un singur utilizator

        //comentarii 
        public virtual ICollection<Comment>? Comments { get; set; }

        //drop down cu categoriile
        [NotMapped] //nu vrem sa mapam cu o coloana in tabel
        public IEnumerable<SelectListItem>? Categ { get; set; }

        //drop down cu subcategoriile
        [NotMapped]
        public IEnumerable<SelectListItem>? Subcateg { get; set; }

        //bookmarks
        public virtual ICollection<ArticleBookmark>? ArticleBookmarks { get; set; }
    }
}

