using System.ComponentModel.DataAnnotations.Schema;

namespace OurApp.Models
{
    public class ArticleBookmark
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

        public int? ArticleId { get; set; }
        public int? BookmarkId { get; set; } 

        public virtual Article? Article { get; set; }   
        public virtual Bookmark? Bookmark { get; set; }   

        public DateTime BookmarkDate { get; set; }
    }
}
