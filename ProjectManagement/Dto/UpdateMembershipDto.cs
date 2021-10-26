using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Dto
{
    public class UpdateMembershipDto
    {
        public int boardId{ get; set; }    
        public int memberId { get; set; }
        public Role newRole { get; set; }

    }
}
