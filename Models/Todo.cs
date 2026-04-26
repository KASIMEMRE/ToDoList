namespace ToDoList.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
