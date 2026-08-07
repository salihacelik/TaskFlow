namespace TaskFlow.API.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; }
    }
}