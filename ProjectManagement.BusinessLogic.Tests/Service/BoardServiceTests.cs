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
        private readonly IUserManager _userManager;
        private readonly BoardService _service;

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

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var claims = new[]
          {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Tom")
            };
            var identity = new ClaimsIdentity(claims, "basic");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext()
            {
                User = principal
            };
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);
            _userManager = new UserMananger(mockHttpContextAccessor.Object, _userRepositoryMock.Object);
            _service = new BoardService(_boardRepositoryMock.Object,_boardMemberRepositoryMock.Object, _userManager);
        }

        [Fact]
        public async void GetBoardsAsync_WithCorrectAnswer()
        {
            //Arrange
            SetupGetBoards(GetBoards());
            //Act
  IEnumerable<Board> boards = await _service.GetBoardsAsync();
            //Assert
            Assert.Equal(3, boards.Count());
        }

        [Fact]
        public async void GetBoardAsync_WithCorrectAnswer()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            List<BoardMember>  boardMembers = new List<BoardMember>();
            BoardMember boardMember = new BoardMember() { UserId = 1 };
           
            boardMembers.Add(boardMember);
            var item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoard(item);
            _boardMemberRepositoryMock.Setup(x=>x.GetSingleAsync(It.IsAny<GetBoardMemberByUserIdSpecification>())).Returns(Task.FromResult(boardMember));
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
            var item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoard(item);
            var memberSpec = new GetBoardMemberByUserIdSpecification(1, boardId);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(memberSpec)).Returns(Task.FromResult(boardMember));
            _boardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(1)).Returns(Task.FromResult(item));

            //Act
            //Assert
            
              await  Assert.ThrowsAsync<WebAppException>(async () => await _service.GetBoardByIdAsync(1));
      
        }
        [Fact]
        public async void GetBoardAsync_BoardNotExist_ExceptionIsThrown()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = 1 };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            var item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoard(item);
            var memberSpec = new GetBoardMemberByUserIdSpecification(1, boardId);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(memberSpec)).Returns(Task.FromResult(boardMember));
            _boardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(1)).Returns(Task.FromResult(item));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetBoardByIdAsync(2));
        }
             [Fact]
        public async void DeleteBoardAsync_ExistingIdPassed_RemovesOneItem()
        {
            //Arrange
            int boardId = 1;
            string boardName = "Board 1";
            BoardMember boardMember = new BoardMember() { UserId = 1 };
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(boardMember);
            var item = new Board
            {
                Id = boardId,
                Name = boardName,
                BoardMembers = boardMembers
            };
            SetupGetBoard(item);         
            _boardRepositoryMock.Setup(repo => repo.DeleteByIdAsync(boardId)).Returns(Task.CompletedTask);
            _boardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.IsAny<GetBoardMemberByUserIdSpecification>())).Returns(Task.FromResult(boardMember));
            // Act
            await _service.DeleteBoardAsync(boardId);
            // Assert
            _boardRepositoryMock.Verify(repo => repo.DeleteByIdAsync(boardId), Times.Once);
        }
        private void SetupGetBoard(Board boards)
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
        private void SetupGetBoardById(Board board)
        {
            _boardRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                            .Returns(Task.FromResult(board));
        }
    }
}
