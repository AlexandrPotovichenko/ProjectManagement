using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.BusinessLogic.Services.Interfaces;

using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _BoardRepository;
        private readonly IUserRepository _UserRepository;
        //private readonly IBoardMemberRepository _BoardMemberRepository;
        public BoardService(IBoardRepository BoardRepository, IUserRepository UserRepository)
        {
            _BoardRepository = BoardRepository;
            _UserRepository = UserRepository;
        }

        //public async Task<IEnumerable<Board>> GetCompletedBoardsAsync(int userId)
        //{
        //    var specification = new GetCompletedBoardsOfUserSpecification(userId);
        //    return await _BoardRepository.GetManyAsync(specification);
        //}

        public async Task<IEnumerable<Board>> GetBoardsAsync()
        {
            //var specification = new GetBoardsSpecification();
            return await _BoardRepository.GetAllAsync();
        }

        //public async Task CompleteBoardAsync(int userId, int BoardId)
        //{
        //    var item = await _BoardRepository.GetByIdAsync(BoardId);

        //    if (item is null)
        //    {
        //        throw new System.Exception();
        //    }

        //    if (item.UserId != userId)
        //    {
        //        throw new System.Exception();
        //    }

        //    item.IsCompleted = true;
        //    await _BoardRepository.UpdateAsync(item);
        //    await _BoardRepository.UnitOfWork.SaveChangesAsync();
        //}

        public async Task<Board> CreateBoardAsync( string name, string description,int userId)
        {
            //var user = await _UserRepository.GetSingleAsync(new GetUserSpecification(userId) );
            var user = await _UserRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new System.Exception();// гозирага
            }
            BoardMember boardMember = new BoardMember(user, Role.Admin);
            var item = new Board(name, description, boardMember);
            //_UserRepository.Entry<Store>(s1).State = EntityState.Detached;
            var insertedItem = await _BoardRepository.InsertAsync(item);
            await _BoardRepository.UnitOfWork.SaveChangesAsync();

            return insertedItem;
        }
    }
}
