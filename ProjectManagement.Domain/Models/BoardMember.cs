using ProjectManagement.Domain.Interfaces;

namespace ProjectManagement.Domain.Models
{
    public class BoardMember : IEntity<int>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Role Role { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; }
        public BoardMember() { }
        public BoardMember(int userId, Role role)
        {
            UserId = userId;
            Role = role;
        }
    }
}