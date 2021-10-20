using ProjectManagement.Domain.Interfaces;

namespace ProjectManagement.Domain.Models
{
    public class CardMember :IEntity<int>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Role Role { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }
        public CardMember(int userId, Role role)
        {
            UserId = userId;
            Role = role;
        }
    }
}