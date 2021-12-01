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
using System.Net;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardMemberRepository _boardMemberRepository;
        private readonly IUserManager _userManager;
        public BoardService(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository, IUserManager userManager)
        {
            _boardRepository = boardRepository;
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
            Board board = new Board(name, description, boardMember);
            Board insertedItem = await _boardRepository.InsertAsync(board);
            await _boardRepository.UnitOfWork.SaveChangesAsync();
            return insertedItem;
        }

        public async Task<Board> GetBoardByIdAsync(int boardId)
        {
            BoardMember currentBoardMember = await GetCurrentBoardMemberAsync(boardId);
            if (!currentBoardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            return await _boardRepository.GetWithItemsByIdAsync(boardId);
        }

        public async Task DeleteBoardAsync(int boardId)
        {
            BoardMember currentBoardMember = await GetCurrentBoardMemberAsync(boardId);
            if (!currentBoardMember.CanDelete)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            await _boardRepository.DeleteByIdAsync(boardId);
        }

        public async Task<IEnumerable<BoardMember>> GetAllBoardMembersAsync(int boardId)
        {
            BoardMember currentBoardMember = await GetCurrentBoardMemberAsync(boardId);
            if (!currentBoardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Board board = await BoardByIdAsync(boardId);
            return board.BoardMembers;
        }

        public async Task<BoardMember> AddMemberToBoardAsync(int newMemberUserId, int boardId, Role role)
        {
            BoardMember currentBoardMember = await GetCurrentBoardMemberAsync(boardId);
            if (!currentBoardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            if (!await _userManager.IsUserExistsAsync(newMemberUserId))
            {
                throw new WebAppException((int)HttpStatusCode.NotFound, $"User with Id {newMemberUserId} not found.");
            }
            Board board = await _boardRepository.GetForEditByIdAsync(boardId);
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
            Guard.Against.NullObject(memberId, boardMember, "BoardMember");
            BoardMember currentBoardMember = await GetCurrentBoardMemberAsync(boardMember.BoardId);
            if (!currentBoardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Board board = await _boardRepository.GetForEditByIdAsync(boardMember.BoardId);
            BoardMember boardMemberForDelete = GetBoardMemberById(board, memberId);
            board.BoardMembers.Remove(boardMemberForDelete);
            await _boardRepository.UpdateAsync(board);
            await _boardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateMembershipOfMemberOnBoardAsync(int boardId, int memberId, Role newRole)
        {
            BoardMember currentBoardMember = await GetCurrentBoardMemberAsync(boardId);
            if (!currentBoardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Board board = await _boardRepository.GetForEditByIdAsync(boardId);
            BoardMember boardMemberForEdit = GetBoardMemberById(board, memberId);
            boardMemberForEdit.Role = newRole;   
            await _boardRepository.UpdateAsync(board);
            await _boardRepository.UnitOfWork.SaveChangesAsync();
        }

        private async Task<Board> BoardByIdAsync(int boardId)
        {
            Board board = await _boardRepository.GetWithMembersAsync(boardId);
            Guard.Against.NullObject(boardId, board, "Board");
            return board;
        }

        private BoardMember GetBoardMemberById(Board board, int memberId)
        {
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberId);
            Guard.Against.NullObject(memberId, boardMember, "BoardMember");
            return boardMember;
        }

        private async Task<BoardMember> GetMemberByUserIdAsync(int boardId, int userId)
        {
            GetBoardMemberByUserIdSpecification memberSpec = new GetBoardMemberByUserIdSpecification(userId, boardId);
            BoardMember boardMember = await _boardMemberRepository.GetSingleAsync(memberSpec);
            Guard.Against.NullObject(userId, boardMember, "BoardMember");        
            return boardMember;
        }

        private async Task<BoardMember> GetCurrentBoardMemberAsync(int boardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            return await GetMemberByUserIdAsync(boardId, currentUserId);
        }
    }
}
