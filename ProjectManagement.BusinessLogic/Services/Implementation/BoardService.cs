using Ardalis.GuardClauses;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Interfaces;

using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Linq;
using System;
using ProjectManagement.BusinessLogic.Specifications;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBoardMemberRepository _boardMemberRepository;
        private readonly IUserManager _userManager;
        public BoardService(IBoardRepository boardRepository, IUserRepository userRepository, IBoardMemberRepository boardMemberRepository, IUserManager userManager)
        {
            _boardRepository = boardRepository;
            _userRepository = userRepository;
            _boardMemberRepository = boardMemberRepository;
            _userManager = userManager;
        }


        public async Task<IEnumerable<Board>> GetBoardsAsync()
        {
            int currentUserId = _userManager.GetCurrentUserId();
            return await _boardRepository.GetWithItemsAsync(currentUserId);
        }

        public async Task<Board> CreateBoardAsync(string name, string description)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember boardMember = new BoardMember(currentUserId, Role.Admin);
            var item = new Board(name, description, boardMember);
            var insertedItem = await _boardRepository.InsertAsync(item);
            await _boardRepository.UnitOfWork.SaveChangesAsync();

            return insertedItem;
        }

        public async Task<Board> GetBoardAsync(int boardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember boardMember = await GetMemberByUserIdAsync(boardId, currentUserId);
            if(!boardMember.CanRead)
            {
                throw new Exception();
            }

            return await _boardRepository.GetWithItemsByIdAsync(boardId);
        }

        public async Task DeleteBoardAsync(int boardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember boardMember = await GetMemberByUserIdAsync(boardId, currentUserId);
            if (!boardMember.CanDelete)
            {
                throw new Exception();
            }
            await _boardRepository.DeleteByIdAsync(boardId);
        }

        public async Task<IEnumerable<BoardMember>> GetMembershipOfMemberOnBoardAsync(int boardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember boardMember = await GetMemberByUserIdAsync(boardId, currentUserId);
            if (!boardMember.CanRead)
            {
                throw new Exception();
            }
            Board board = await GetBoardByIdAsync(boardId);
            return board.BoardMembers;
        }

        public async Task<BoardMember> AddMemberToBoardAsync(int newMemberUserId, int boardId, Role role)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember boardMember = await GetMemberByUserIdAsync(boardId, currentUserId);
            if (!boardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            Board board = await _boardRepository.GetForEditByIdAsync(boardId);

            User user = await _userRepository.GetByIdAsync(newMemberUserId);
            Guard.Against.NullObject(newMemberUserId, user, "User");
            Guard.Against.CheckMemebershipBoard(newMemberUserId, board);
          BoardMember newBoardMember = new BoardMember(newMemberUserId, role);

            board.BoardMembers.Add(newBoardMember);

            await _boardRepository.UpdateAsync(board);
            await _boardRepository.UnitOfWork.SaveChangesAsync();
            return newBoardMember;
        }

        public async Task RemoveMemberFromBoardAsync(int memberId)
        {
            BoardMember boardMember = await _boardRepository.GetBoardMemberByIdAsync(memberId);

            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember currentBoardMember = await GetMemberByUserIdAsync(boardMember.BoardId, currentUserId);
            if (!currentBoardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            Board board = await _boardRepository.GetForEditByIdAsync(boardMember.BoardId);
            BoardMember removableBoardMember = GetBoardMemberByMemberId(board, memberId);

            board.BoardMembers.Remove(removableBoardMember);

            await _boardRepository.UpdateAsync(board);
            await _boardRepository.UnitOfWork.SaveChangesAsync();

        }

        public async Task UpdateMembershipOfMemberOnBoardAsync(int boardId, int memberId, Role newRole)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            BoardMember boardMember = await GetMemberByUserIdAsync(boardId, currentUserId);
            if (!boardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            Board board = await _boardRepository.GetForEditByIdAsync(boardId);
            BoardMember editableBoardMember = GetBoardMemberByMemberId(board, memberId);

            editableBoardMember.Role = newRole;
        
            await _boardRepository.UpdateAsync(board);
            await _boardRepository.UnitOfWork.SaveChangesAsync();
        }

        private async Task<Board> GetBoardByIdAsync(int boardId)
        {
            Board board = await _boardRepository.GetWithMembersAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board");
            return board;
        }
        private BoardMember GetBoardMemberByMemberId(Board board, int memberId)
        {
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberId);
            Guard.Against.NullObject(memberId, boardMember, "BoardMember");
            return boardMember;
        }
        private async Task<BoardMember> GetMemberByUserIdAsync(int boardId, int userId)
        {
            var memberSpec = new GetBoardMemberByUserIdSpecification(userId, boardId);
            BoardMember boardMember = await _boardMemberRepository.GetSingleAsync(memberSpec);
            Guard.Against.NullObject(userId, boardMember, "BoardMember");
            
            return boardMember;
        }

    }
}
