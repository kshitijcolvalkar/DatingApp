namespace DattingApp.API.Models
{
    public class Like
    {
        public int LikeeId { get; set; }
        public int LikerId { get; set; }
        public virtual User Likee { get; set; }
        public virtual User Liker { get; set; }
    }
}