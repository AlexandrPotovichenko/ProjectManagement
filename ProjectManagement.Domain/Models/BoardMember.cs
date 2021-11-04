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
        public bool CanRead
        {
            get {return true; }          
        }
        public bool CanUpdate
        {
            get
            {
                if (Role == Role.Normal || Role == Role.Admin)
                    return true;
                else
                    return false;
            }
        }
        public bool CanDelete
        {
            get
            {
                if (Role == Role.Admin)
                    return true;
                else
                    return false;
            }
        }
        public bool IsMemberAdmin
        {
            get
            {
                if (Role == Role.Admin)
                    return true;
                else
                    return false;
            }
        }
        
    }
}