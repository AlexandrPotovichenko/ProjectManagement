using Ardalis.GuardClauses;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.BusinessLogic.Services.Interfaces;

using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Linq;
using System;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _BoardRepository;
        private readonly IUserRepository _UserRepository;
        public BoardService(IBoardRepository BoardRepository, IUserRepository UserRepository)
        {
            _BoardRepository = BoardRepository;
            _UserRepository = UserRepository;
        }


        public async Task<IEnumerable<Board>> GetBoardsAsync()
        {
            return await _BoardRepository.GetWithItemsAsync();
        }

        public async Task<Board> CreateBoardAsync( string name, string description,int userId)
        {
            BoardMember boardMember = new BoardMember(userId, Role.Admin);
            var item = new Board(name, description, boardMember);
            var insertedItem = await _BoardRepository.InsertAsync(item);
            await _BoardRepository.UnitOfWork.SaveChangesAsync();

            return insertedItem;
        }

        public async Task<Board> GetBoardAsync(int boardId)
        {
               return await _BoardRepository.GetByIdAsync(boardId);
        }

        public async Task DeleteBoard(int boardId)
        {
            Board board = await _BoardRepository.GetByIdAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board");

            await _BoardRepository.DeleteByIdAsync(boardId);
        }

        public async Task<IEnumerable<BoardMember>> GetMembershipOfMemberOnBoard(int boardId)
        {
            Board board = await _BoardRepository.GetByIdAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board"); 
            
            return board.BoardMembers;
        }

        public async Task AddMemberToBoard(int userId, int boardId, Role role)
        {

            Board board = await _BoardRepository.GetByIdAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board");

            User user = await _UserRepository.GetByIdAsync(userId);
            Guard.Against.NullObject(userId, user, "User");
            Guard.Against.CheckMemebershipBoard(userId, board);
                
            BoardMember boardMember = new BoardMember(userId, role);
            board.BoardMembers.Add(boardMember);

            await _BoardRepository.UpdateAsync(board);
            await _BoardRepository.UnitOfWork.SaveChangesAsync();

        }

        public async Task RemoveMemberFromBoard(int memberId, int boardId)
        {
            Board board = await _BoardRepository.GetByIdAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board");

            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberId);
            Guard.Against.NullObject(memberId, boardMember, "BoardMember");

            board.BoardMembers.Remove(boardMember);

            await _BoardRepository.UpdateAsync(board);
            await _BoardRepository.UnitOfWork.SaveChangesAsync();

        }

        public async Task UpdateMembershipOfMemberOnBoard(int boardId, int memberId, Role newRole)
        {
            Board board = await _BoardRepository.GetByIdAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board");

            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberId);
            Guard.Against.NullObject(memberId, boardMember, "BoardMember");

            boardMember.Role = newRole;
        
            await _BoardRepository.UpdateAsync(board);
            await _BoardRepository.UnitOfWork.SaveChangesAsync();
        }
    }
}
