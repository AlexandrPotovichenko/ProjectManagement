using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Implementation;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProjectManagement.BusinessLogic.Tests.Service
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IBoardMemberRepository> _boardMemberRepositoryMock;
        private readonly Mock<ICardMemberRepository> _cardMemberRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly UserService _service;
        private int _currentUserId = 1;
        public UserServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));
            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);
            _userManagerMock = new Mock<IUserManager>();
            _userManagerMock.Setup(x => x.GetCurrentUserId()).Returns(_currentUserId);
            _boardMemberRepositoryMock = new Mock<IBoardMemberRepository>();
            _cardMemberRepositoryMock = new Mock<ICardMemberRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _service = new UserService(_userRepositoryMock.Object, _userManagerMock.Object, _boardMemberRepositoryMock.Object, _cardMemberRepositoryMock.Object, _fileServiceMock.Object);
        }
        [Fact]
        public async void AuthenticateUserAsync_CorrectLoginAndPassword_Passed()
        {
            //Arrange
            string login = "FakeLogin";
            string password = "FakePassword";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            _userRepositoryMock.Setup(x => x.GetByNameAsync(login))
                            .Returns(Task.FromResult(user));
            //Act
            User userResult = await _service.AuthenticateUserAsync(login, password);
            //Assert
            Assert.NotNull(userResult);
            Assert.Equal(login, userResult.Name);
        }
        [Fact]
        public async void AuthenticateUserAsync_NotCorrectLogin_ExceptionIsThrown()
        {
            //Arrange
            string login = "FakeLogin";
            string wrongLogin = "FakeLogin1";
            string password = "FakePassword";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            _userRepositoryMock.Setup(x => x.GetByNameAsync(login))
                            .Returns(Task.FromResult(user));
            //Act
            var exception = await Record.ExceptionAsync(() => _service.AuthenticateUserAsync(wrongLogin, password));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("Login or password is incorrect.", exception.Message);
        }
        [Fact]
        public async void AuthenticateUserAsync_NotCorrectPassword_ExceptionIsThrown()
        {
            //Arrange
            string login = "FakeLogin";
            string wrongPassword = "FakePassword123";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            _userRepositoryMock.Setup(x => x.GetByNameAsync(login))
                            .Returns(Task.FromResult(user));
            //Act
            var exception = await Record.ExceptionAsync(() => _service.AuthenticateUserAsync(login, wrongPassword));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("Login or password is incorrect.", exception.Message);
        }
        [Fact]
        public async void RegisterUserAsync_CorrectLoginAndPassword_Passed()
        {
            //Arrange
            string login = "FakeLogin";
            string password = "FakePassword";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            User currentUser = new User
            {
                Id = 1,
                Name = "admin",
                CanAdministerUsers = true
            };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_currentUserId)).Returns(Task.FromResult(currentUser));
            _userRepositoryMock.Setup(x => x.InsertAsync(It.Is<User>(u => u.Name == login)))
                            .Returns(Task.FromResult(user));
            //Act
            User userResult = await _service.RegisterUserAsync(login, password);
            //Assert
            Assert.NotNull(userResult);
            Assert.Equal(login, userResult.Name);
        }
        [Fact]
        public async void RegisterUserAsync_UsernameAlreadyExists_ExceptionIsThrown()
        {
            //Arrange
            string existingLogin = "FakeLogin";
            string password = "FakePassword";
            User currentUser = new User
            {
                Id = 1,
                Name = "admin",
                CanAdministerUsers = true
            };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_currentUserId)).Returns(Task.FromResult(currentUser));
            _userRepositoryMock.Setup(x => x.IsUserExistsAsync(existingLogin))
                            .Returns(Task.FromResult(true));
            //Act
            var exception = await Record.ExceptionAsync(() => _service.RegisterUserAsync(existingLogin, password));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("A user with this login already exists!", exception.Message);
            _userRepositoryMock.Verify(service => service.InsertAsync(It.IsAny<User>()), Times.Never);
        }
        [Fact]
        public async void UploadAvatarAsync_CorrectFile_Passed()
        {
            //Arrange
            int userId = 1;
            string content = "Hello World from a Fake File";
            string login = "FakeLogin";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            _userRepositoryMock.Setup(x => x.GetForEditByIdAsync(userId)).Returns(Task.FromResult(user));
            using (var stream = GenerateStreamFromString(content))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("testFile.png"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/png"
                };
            //Act
                var exception = await Record.ExceptionAsync(() => _service.UploadAvatarAsync(userId, file));
            //Assert
                _fileServiceMock.Verify(x => x.CheckFileForAvatar(It.IsAny<IFormFile>()), Times.Once);
                _fileServiceMock.Verify(x => x.ScanFileForVirusesAsync(It.IsAny<IFormFile>()), Times.Once);
                Assert.Null(exception);
            }
        }
        [Fact]
        public async void UploadAvatarAsync_NotExistingUserId_ExceptionIsThrown()
        {
            //Arrange
            int userId = 1;
            int wrongUserId = 2;
            string content = "Hello World from a Fake File";
            string login = "FakeLogin";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            User currentUser = new User
            {
                Id = 1,
                Name = "admin",
                CanAdministerUsers = true
            };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_currentUserId)).Returns(Task.FromResult(currentUser));
            _userRepositoryMock.Setup(x => x.GetForEditByIdAsync(userId)).Returns(Task.FromResult(user));
            using (var stream = GenerateStreamFromString(content))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("testFile.png"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/png"
                };
            //Act
                var exception = await Record.ExceptionAsync(() => _service.UploadAvatarAsync(wrongUserId, file));
            //Assert
                _fileServiceMock.Verify(x => x.CheckFileForAvatar(It.IsAny<IFormFile>()), Times.Never);
                _fileServiceMock.Verify(x => x.ScanFileForVirusesAsync(It.IsAny<IFormFile>()), Times.Never);
                Assert.NotNull(exception);
                Assert.Equal("A user can only upload an Avatar for his own profile.", exception.Message);
            }
        }
        [Fact]
        public async void UploadAvatarAsync_NotCorrectTypeOfFile_ExceptionIsThrown()
        {
            //Arrange
            int userId = 1;
            string content = "Hello World from a Fake File";
            string login = "FakeLogin";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            string fileName = "testFile.txt";
            User user = new User(login, passwordHash);
            _fileServiceMock.Setup(x => x.CheckFileForAvatar(It.Is<IFormFile>(f => f.FileName == fileName))).Throws<WebAppException>();
            _userRepositoryMock.Setup(x => x.GetForEditByIdAsync(userId)).Returns(Task.FromResult(user));
            using (var stream = GenerateStreamFromString(content))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(fileName))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/txt"
                };
            //Act
                var exception = await Record.ExceptionAsync(() => _service.UploadAvatarAsync(userId, file));
            //Assert
                _fileServiceMock.Verify(x => x.CheckFileForAvatar(It.IsAny<IFormFile>()), Times.Once);
                _fileServiceMock.Verify(x => x.ScanFileForVirusesAsync(It.IsAny<IFormFile>()), Times.Never);
                Assert.NotNull(exception);
                Assert.Equal("Exception of type 'ProjectManagement.BusinessLogic.Exceptions.WebAppException' was thrown.", exception.Message);
            }
        }
        [Fact]
        public async void UploadAvatarAsync_EmptyFile_ExceptionIsThrown()
        {
            //Arrange
            int userId = 1;
            string login = "FakeLogin";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            IFormFile file = null;
            User user = new User(login, passwordHash);
            _fileServiceMock.Setup(x => x.CheckFileForAvatar(It.Is<IFormFile>(f => f == null))).Throws<WebAppException>();
            _userRepositoryMock.Setup(x => x.GetForEditByIdAsync(userId)).Returns(Task.FromResult(user));
            //Act
            var exception = await Record.ExceptionAsync(() => _service.UploadAvatarAsync(userId, file));
            //Assert
            _fileServiceMock.Verify(x => x.CheckFileForAvatar(It.IsAny<IFormFile>()), Times.Once);
            _fileServiceMock.Verify(x => x.ScanFileForVirusesAsync(It.IsAny<IFormFile>()), Times.Never);
            Assert.NotNull(exception);
            Assert.Equal("Exception of type 'ProjectManagement.BusinessLogic.Exceptions.WebAppException' was thrown.", exception.Message);
        }
        [Fact]
        public async void UploadAvatarAsync_InfectedFile_ExceptionIsThrown()
        {
            //Arrange
            int userId = 1;
            string content = "Hello World from a Fake File";
            string login = "FakeLogin";
            string fileName = "testFile.png";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash);
            _fileServiceMock.Setup(x => x.ScanFileForVirusesAsync(It.Is<IFormFile>(f=>f.FileName== fileName))).Throws<WebAppException>();
            _userRepositoryMock.Setup(x => x.GetForEditByIdAsync(userId)).Returns(Task.FromResult(user));
            using (var stream = GenerateStreamFromString(content))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(fileName))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/png"
                };
            //Act
                var exception = await Record.ExceptionAsync(() => _service.UploadAvatarAsync(userId, file));
            //Assert
                _fileServiceMock.Verify(x => x.CheckFileForAvatar(It.IsAny<IFormFile>()), Times.Once);
                _fileServiceMock.Verify(x => x.ScanFileForVirusesAsync(It.IsAny<IFormFile>()), Times.Once);
                Assert.NotNull(exception);
                Assert.Equal("Exception of type 'ProjectManagement.BusinessLogic.Exceptions.WebAppException' was thrown.", exception.Message);
            }
        }
        [Fact]
        public async void DownloadAvatarAsync_FileExists_Passed()
        {
            //Arrange
            int userId = 1;
            int avatarId = 1;
            string login = "FakeLogin";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash) { Id = userId };
            user.Avatar = new AppFile("testFile.png", "application/png", null) { Id = avatarId };
            _userRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(userId)).Returns(Task.FromResult(user));
            //Act
            AppFile appFile = await _service.DownloadAvatarAsync(userId);
            //Assert
            Assert.NotNull(appFile);
            Assert.Equal(user.Avatar, appFile);
        }
        [Fact]
        public async void DownloadAvatarAsync_NotExistingUserId_ExceptionIsThrown()
        {
            //Arrange
            int userId = 1;
            int wrongUserId = 2;
            int avatarId = 1;
            string login = "FakeLogin";
            string passwordHash = @"$2a$11$T38ECJwdEaDqGJBonPhmN.SA6q6CyHVJq5.XjTBY/MNOp78lbUjgG";
            User user = new User(login, passwordHash) { Id = userId };
            user.Avatar = new AppFile("testFile.png", "application/png", null) { Id = avatarId };
            _userRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(userId)).Returns(Task.FromResult(user));
            //Act
            var exception = await Record.ExceptionAsync(() => _service.DownloadAvatarAsync(wrongUserId));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("No User found with id 2", exception.Message);     
        }
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
