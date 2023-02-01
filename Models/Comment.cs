using System;
using System.ComponentModel.DataAnnotations;

namespace OurApp.Models
{
    public class Comment
    {
        //id = primary key
        [Key]
        public int Id { get; set; }

        //continut + restrictii
        [Required(ErrorMessage = "Continutul este obligatoriu!")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int? ArticleId { get; set; }

        //user = foreign key 
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } //un comentariu are un singur utilizator

        public virtual Article? Article { get; set; }
    }
}

