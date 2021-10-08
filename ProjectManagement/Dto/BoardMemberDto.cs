using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class BoardMemberDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Role Role { get; set; }
        public int BoardId { get; set; }

    }
}