using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class BoardMemberDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}