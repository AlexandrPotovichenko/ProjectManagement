using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class PostCardMemberDto
    {
        public int UserId { get; set; }
        public Role Role { get; set; }
        public int CardId { get; set; }
    }
}