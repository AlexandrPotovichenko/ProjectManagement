using ProjectManagement.Domain.Interfaces;

namespace ProjectManagement.Domain.Models
{
    public class BoardMember : IEntity<int>
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; }
        public BoardMember() { }
        public BoardMember(User user, Role role)
        {
            User = user;
            Role = role;
        }
    }
}