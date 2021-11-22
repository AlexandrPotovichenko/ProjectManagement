using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Services.Implementation;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using Xunit;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.BusinessLogic.Exceptions;

namespace ProjectManagement.BusinessLogic.Tests.Service
{
    public class BoardServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBoardRepository> _boardRepositoryMock;
        private readonly Mock<IBoardMemberRepository> _boardMemberRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly BoardService _service;

        private int _currentUserId = 1;
       
        public BoardServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            _boardRepositoryMock = new Mock<IBoardRepository>();
            _boardRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _boardMemberRepositoryMock = new Mock<IBoardMemberRepository>();
            _boardMemberRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _userManagerMock = new Mock<IUserManager>();
            _userManagerMock.Setup(x => x.GetCurrentUserId()).Returns(_currentUserId);
            
            _service = new BoardService(_boardRepositoryMock.Object, _boardMemberRepositoryMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async void GetBoardsAsync_BoardsExist_Passed()
        {
            //Arrange
            SetupGetBoards(GetBoards());
            //Act
            IEnumerable<Board> boards = await _service.GetBoardsAsync();
            //Assert
            Assert.Equal(3, boards.Count());
        }
        [Fact]
        public async void GetBoardAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            List<BoardMember> boardMembers = new List<BoardMember>();
            BoardMember boardMember = new BoardMember() { UserId = 1 };

            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoardById(item);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.IsAny<GetBoardMemberByUserIdSpecification>())).Returns(Task.FromResult(boardMember));
            _boardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(item));
            //Act
            Board board = await _service.GetBoardByIdAsync(1);
            //Assert
            Assert.Equal(item, board);
        }
        [Fact]
        public async void GetBoardAsync_UserIsNotAnMemberOfBoard_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = 2 };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoardById(item);
            GetBoardMemberByUserIdSpecification memberSpec = new GetBoardMemberByUserIdSpecification(1, boardId);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(memberSpec)).Returns(Task.FromResult(boardMember));
            _boardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(1)).Returns(Task.FromResult(item));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetBoardByIdAsync(1));
            _boardRepositoryMock.Verify(service => service.GetWithItemsAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetBoardAsync_BoardNotExist_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            int wrongBoardId = 2;
            int userId = 1;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = userId };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoardById(item);
            GetBoardMemberByUserIdSpecification memberSpec = new GetBoardMemberByUserIdSpecification(userId, boardId);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(memberSpec)).Returns(Task.FromResult(boardMember));
            _boardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(1)).Returns(Task.FromResult(item));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetBoardByIdAsync(wrongBoardId));
            _boardRepositoryMock.Verify(service => service.GetWithItemsAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void DeleteBoardAsync_UserIsNotABoardMember_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            int UserId = 2;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = 2 };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoardById(item);
            _boardRepositoryMock.Setup(repo => repo.DeleteByIdAsync(boardId)).Returns(Task.CompletedTask);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(bs => bs.BoardId == boardId && bs.UserId == UserId)))
                .Returns(Task.FromResult(boardMember));
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteBoardAsync(boardId));
            _boardRepositoryMock.Verify(service => service.DeleteByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void CreateBoardAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            string description = "description Board 1";
            BoardMember boardMember = new BoardMember() { UserId = 1, Role = Role.Admin };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers,
                Description = description
            };
            _boardRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<Board>())).Returns(Task.FromResult(item));
            // Act
            Board board = await _service.CreateBoardAsync(boardName, description);
            // Assert
            Assert.NotNull(board);
            Assert.Equal(boardId, board.Id);
        }
        [Fact]
        public async void DeleteBoardAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = 1, Role = Role.Admin };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoardById(item);
            _boardRepositoryMock.Setup(repo => repo.DeleteByIdAsync(boardId)).Returns(Task.CompletedTask);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.IsAny<GetBoardMemberByUserIdSpecification>())).Returns(Task.FromResult(boardMember));
            // Act
            await _service.DeleteBoardAsync(boardId);
            // Assert
            _boardRepositoryMock.Verify(repo => repo.DeleteByIdAsync(boardId), Times.Once);
        }
        [Fact]
        public async void DeleteBoardAsync_NotExistingId_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            int wrongBoardId = 2;
            int userId = 1;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = userId, Role = Role.Admin };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            Board item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoardById(item);
            _boardRepositoryMock.Setup(repo => repo.DeleteByIdAsync(boardId)).Returns(Task.CompletedTask);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(bm=>bm.BoardId == boardId && bm.UserId== userId)))
                .Returns(Task.FromResult(boardMember));
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteBoardAsync(wrongBoardId));
            _boardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetAllBoardMembersAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            int userId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(m => m.UserId == 1);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(x => x.GetWithMembersAsync(boardId)).Returns(Task.FromResult(board));
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms=>ms.BoardId == boardId&&ms.UserId == userId)))
                .Returns(Task.FromResult(boardMember));
            // Act
            IEnumerable<BoardMember> members = await _service.GetAllBoardMembersAsync(boardId);
            // Assert
            Assert.Collection(members,
                item => Assert.Equal(1, item.UserId),
                item => Assert.Equal(2, item.UserId),
                item => Assert.Equal(3, item.UserId)
            );
            Assert.Equal(members.Count(), board.BoardMembers.Count);
        }
        [Fact]
        public async void GetAllBoardMembersAsync_BoardNotExist_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(x => x.GetWithMembersAsync(boardId)).Returns(Task.FromResult(board));
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetAllBoardMembersAsync(boardId));
            _boardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetAllBoardMembersAsync_UserIsNotABoardMember_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(m => m.UserId == 1);
            board.BoardMembers.Remove(boardMember);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(x => x.GetWithMembersAsync(boardId)).Returns(Task.FromResult(board));
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetAllBoardMembersAsync(boardId));
            _boardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void AddMemberToBoardAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember currentMember = board.BoardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            int newMemberUserId = 4;
            Role role = Role.Normal;
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentMember));
            _userManagerMock.Setup(x => x.IsUserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _userRepositoryMock.Setup(x => x.UserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b=>b.BoardMembers.Any(bm=>bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            // Act
            BoardMember boardMember = await _service.AddMemberToBoardAsync(newMemberUserId, boardId, role);
            // Assert
            Assert.NotNull(boardMember);
            Assert.Equal(newMemberUserId, boardMember.UserId);
        }
        [Fact]
        public async void AddMemberToBoardAsync_NotExistingUserId_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember currentMember = board.BoardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            int newMemberUserId = 4;
            Role role = Role.Normal;
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentMember));
            // here we will return false, which means that a user with such an ID does not exist 
            _userRepositoryMock.Setup(x => x.UserExistsAsync(newMemberUserId)).Returns(Task.FromResult(false));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.BoardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddMemberToBoardAsync(newMemberUserId, boardId, role));
            _boardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Board>()), Times.Never);
        }
        [Fact]
        public async void AddMemberToBoardAsync_UserIsNotAnAdmin_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember currentMember = board.BoardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            // change the role of the current user
            currentMember.Role = Role.Observer;
            int newMemberUserId = 4;
            Role role = Role.Normal;
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentMember));
            // return true, which means that a user with such an ID exists
            _userRepositoryMock.Setup(x => x.UserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.BoardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddMemberToBoardAsync(newMemberUserId, boardId, role));
            _boardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Board>()), Times.Never);
        }
        [Fact]
        public async void AddMemberToBoardAsync__NotExistingBoardId_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember currentMember = board.BoardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            int newMemberUserId = 4;
            Role role = Role.Normal;
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentMember));
            // return true, which means that a user with such an ID exists
            _userRepositoryMock.Setup(x => x.UserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.BoardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            // Act
            // Assert
            //increase boardId by one 
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddMemberToBoardAsync(newMemberUserId, boardId+1, role));
            _boardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Board>()), Times.Never);
        }
        [Fact]
        public async void RemoveMemberFromBoardAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            int memberIdForDelete = 2;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForDelete = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(repo => repo.GetBoardMemberByIdAsync(boardMemberForDelete.Id)).Returns(Task.FromResult(boardMemberForDelete));
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            await _service.RemoveMemberFromBoardAsync(memberIdForDelete);
            // Assert
            Assert.DoesNotContain(board.BoardMembers, b => b.Id == memberIdForDelete);
        }
        [Fact]
        public async void RemoveMemberFromBoardAsync_NotExistingBoardMemberId_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            int memberIdForDelete = 5;// not exist
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForDelete = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            SetupGetBoardById(board);
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.RemoveMemberFromBoardAsync(memberIdForDelete));
            ////
            _boardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Board>()), Times.Never);
        }
        [Fact]
        public async void RemoveMemberFromBoardAsync_UserIsNotAnAdmin_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            int memberIdForDelete = 2;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForDelete = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            // set current BoardMember new role
            currentBoardMember.Role = Role.Observer;
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(repo => repo.GetBoardMemberByIdAsync(boardMemberForDelete.Id)).Returns(Task.FromResult(boardMemberForDelete));
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.RemoveMemberFromBoardAsync(memberIdForDelete));
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnBoardAsync_ExistingId_Passed()
        {
            //Arrange
            int boardId = 1;
            int memberIdForUpdate = 2;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForUpdate = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForUpdate);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(repo => repo.GetBoardMemberByIdAsync(memberIdForUpdate)).Returns(Task.FromResult(boardMemberForUpdate));
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            await _service.UpdateMembershipOfMemberOnBoardAsync(boardId, memberIdForUpdate, Role.Observer);
            // Assert
            Assert.Equal(Role.Observer, board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForUpdate)?.Role);
        }

        [Fact]
        public async void UpdateMembershipOfMemberOnBoardAsync_NotExistingBoardId_ExceptionIsThrown()
        {
            //Arrange
            int notExistingBoardId = 5;
            int boardId = 1;
            int memberIdForUpdate = 2;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForUpdate = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForUpdate);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(repo => repo.GetBoardMemberByIdAsync(memberIdForUpdate)).Returns(Task.FromResult(boardMemberForUpdate));
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.UpdateMembershipOfMemberOnBoardAsync(notExistingBoardId, memberIdForUpdate, Role.Observer));
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnBoardAsync_NotExistingBoardMemberId_ExceptionIsThrown()
        {
            //Arrange
            int notExistingMemberId = 5;
            int boardId = 1;
            int memberIdForUpdate = 2;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForUpdate = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForUpdate);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(repo => repo.GetBoardMemberByIdAsync(memberIdForUpdate)).Returns(Task.FromResult(boardMemberForUpdate));
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.UpdateMembershipOfMemberOnBoardAsync(boardId, notExistingMemberId, Role.Observer));
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnBoardAsync_UserIsNotAnAdmin_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            int memberIdForUpdate = 2;
            string boardName = "Board 1";
            Board board = GetBoardWithMembers(boardId, boardName);
            BoardMember boardMemberForUpdate = board.BoardMembers.FirstOrDefault(bm => bm.Id == memberIdForUpdate);
            BoardMember currentBoardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            // set current BoardMember new role
            currentBoardMember.Role = Role.Observer;
            SetupGetBoardById(board);
            _boardRepositoryMock.Setup(repo => repo.GetBoardMemberByIdAsync(memberIdForUpdate)).Returns(Task.FromResult(boardMemberForUpdate));
            //GetCurrentBoardMember
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetBoardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.BoardId == boardId)))
                .Returns(Task.FromResult(currentBoardMember));
            _boardRepositoryMock.Setup(x => x.GetForEditByIdAsync(boardId)).Returns(Task.FromResult(board));
            _boardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Board>(b => b.Id == boardId))).Returns(Task.CompletedTask);
            // Act
            // Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.UpdateMembershipOfMemberOnBoardAsync(boardId, memberIdForUpdate, Role.Observer));
        }


        private void SetupGetBoardById(Board boards)
        {
            _boardRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                            .Returns(Task.FromResult(boards));
        }
        private void SetupGetBoards(IEnumerable<Board> boards)
        {
            _boardRepositoryMock.Setup(x => x.GetWithItemsAsync(It.IsAny<int>()))
                            .Returns(Task.FromResult(boards));
        }
        private List<Board> GetBoards()
        {
            List<Board> boards = new List<Board>();
            boards.Add(new Board() { Id = 1, Name = "Board 1" });
            boards.Add(new Board() { Id = 2, Name = "Board 2" });
            boards.Add(new Board() { Id = 3, Name = "Board 3" });
            return boards;
        }
        private Board GetBoardWithMembers(int boardId, string boardName)
        {
            BoardMember boardMember;
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMember = new BoardMember() { Id = 1, UserId = 1, Role = Role.Admin, BoardId = boardId};
            boardMembers.Add(boardMember);
            boardMember = new BoardMember() { Id = 2, UserId = 2, Role = Role.Normal, BoardId = boardId};
            boardMembers.Add(boardMember);
            boardMember = new BoardMember() { Id = 3, UserId = 3, Role = Role.Observer, BoardId = boardId};
            boardMembers.Add(boardMember);
            Board board = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            return board;
        }
    }
}
