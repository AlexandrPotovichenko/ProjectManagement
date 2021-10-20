using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class PostBoardMemberDto
    {
        public int UserId { get; set; }
        public Role Role { get; set; }
        public int BoardId { get; set; }

    }
}