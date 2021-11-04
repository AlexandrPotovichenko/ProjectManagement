using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface IBoardMemberRepository : IRepository<BoardMember, int>
    {
    }
}
