using Microsoft.EntityFrameworkCore;
using ProjectManagement.DataAccess.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory;
using ProjectManagement.BusinessLogic.Services.Implementation;
using ProjectManagement.DataAccess.Repositories.Implementation;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProjectManagement.Domain.Models;
using Xunit;
using ProjectManagement.BusinessLogic.Exceptions;

namespace ProjectManagement.BusinessLogic.Tests.Service
{
    public class BoardServiceTests_InMemory
    {
        private DbContextOptions<ProjectManagementContext> dbContextOptions = new DbContextOptionsBuilder<ProjectManagementContext>()
       .UseInMemoryDatabase(databaseName: "ProjectManagementDb")
       .Options;
        private BoardService _boardService;

        public BoardServiceTests_InMemory()
        {
            ProjectManagementContext projectManagementContext = new ProjectManagementContext(dbContextOptions);
            if (projectManagementContext.Boards.Count() == 0)
                SeedDb();

            //Mock IHttpContextAccessor
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

            var _userManager = new UserMananger(mockHttpContextAccessor.Object, new UserRepository(projectManagementContext));

            _boardService = new BoardService(new BoardRepository(projectManagementContext), new BoardMemberRepository(projectManagementContext), _userManager);

        }

        private void SeedDb()
        {
            int userId = 1;
            List<BoardMember> boardMembers = new List<BoardMember>();
            boardMembers.Add(new BoardMember(userId, Role.Admin));
            using (var context = new ProjectManagementContext(dbContextOptions))
            {


                List<Board> boards = new List<Board>
        {
            new Board { Id = 1, Name = "Board 1",BoardMembers = boardMembers},
            new Board { Id = 2, Name = "Board 2" },
            new Board { Id = 3, Name = "Board 3"}
        };

                context.AddRange(boards);
                context.SaveChanges();
            }
        }


        [Fact]
        public async void GetBoardsAsync_Passed()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                //Act
                
                List<Board> boards = (await _boardService.GetBoardsAsync()).ToList();
                //Assert
                // there is one board with a member with ID 1 in the database by default 
                Assert.Single(boards);
            }
        }


        [Fact]
        public async void GetBoardByIdAsync_ExistingId_Passed()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                int boardId = 1;
                //Act
                Board board = await _boardService.GetBoardByIdAsync(boardId);
                //Assert
                Assert.NotNull(board);
            }
        }

        [Fact]
        public async void GetBoardByIdAsync_NotExistingId_ExceptionIsThrown()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                int boardId = 5;
                //Act
                //Assert
                await Assert.ThrowsAsync<WebAppException>(async () => await _boardService.GetBoardByIdAsync(boardId));
            }
        }

        [Fact]
        public async void DeleteBoardAsync_NotExistingId_ExceptionIsThrown()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                int boardId = 1;
                //Act
                Board boardForDelete = await _boardService.GetBoardByIdAsync(boardId);
                //Assert
                await Assert.ThrowsAsync<WebAppException>(async () => await _boardService.DeleteBoardAsync(boardForDelete.Id + 1));
            }
        }

        [Fact]
        public async void DeleteBoardAsync_UserIsNotABoardMember_ExceptionIsThrown()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                int boardId = 2;
                //Act
                //Assert
                await Assert.ThrowsAsync<WebAppException>(async () => await _boardService.DeleteBoardAsync(boardId));
            }
        }
        [Fact]
        public async void DeleteBoardAsync_ExistingId_Passed()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                int userId = 1;
                BoardMember boardMember = new BoardMember(userId, Role.Admin);
                Board boardForDelete = new Board { Id = 555, Name = "Board 555" };
                boardForDelete.BoardMembers.Add(boardMember);
                context.Boards.Add(boardForDelete);
                context.SaveChanges();
                Board board = await _boardService.GetBoardByIdAsync(boardForDelete.Id);
                //Act
                await _boardService.DeleteBoardAsync(boardForDelete.Id);
                //Assert
                Assert.DoesNotContain(board, await _boardService.GetBoardsAsync());
            }
        }
        [Fact]
        public async void CreateBoardAsync_Passed()
        {
            using (var context = new ProjectManagementContext(dbContextOptions))
            {
                //Arrange
                string name = "BoardName 123";
                string description = "Description 123";
                //Act
                Board board = await _boardService.CreateBoardAsync(name,description);
                //Assert
                Assert.NotNull(board);

                await _boardService.DeleteBoardAsync(board.Id);
            }
        }
    }
}
