using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OurApp.Models
{
    public class ArticlesHistory
    {
        //id = primary key
        [Key]
        public int Id { get; set; }
        public int ArticleId { get; set; }
        
        public string? Title { get; set; }

        public string? Content { get; set; }

        //create date
        public DateTime? EditDate { get; set; }

        //last update

        //categoria = foreign key
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        //subcategoria = foreign key
        public int? SubcategoryId { get; set; }
        public virtual Subcategory? Subcategory { get; set; }

        //user = foreign key 
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } //un articol are un singur utilizator
    
    }
}
