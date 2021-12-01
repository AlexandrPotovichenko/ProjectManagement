using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProjectManagement.BusinessLogic.Options;
using ProjectManagement.BusinessLogic.Services.Implementation;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ProjectManagement.BusinessLogic.Tests.Service
{
    public class FileServiceTests
    {
        private readonly Mock<ILogger<FileService>> _loggerMock;
        //private readonly ClamAVServerOptions _options;
        //private readonly Mock<IConfiguration> _configurationMock;
        private readonly FileService _service;
        public FileServiceTests()
        {
            _loggerMock = new Mock<ILogger<FileService>>();
            var optionsMock = new Mock<IOptions<ClamAVServerOptions>>();
            optionsMock.SetupGet(o => o.Value).Returns(new ClamAVServerOptions() { URL = "127.0.0.1", Port = "3310" });
            _service = new FileService(_loggerMock.Object, optionsMock.Object);
        }
        [Fact]
        public async void ScanFileForVirusesAsync_NotInfectedFile_Passed()
        {
            //Arrange
            string text = "a,b \n c,d";
            using (var stream = GenerateStreamFromString(text))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("Test.txt"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/txt"
                };
            //Act
                var exception = await Record.ExceptionAsync(async () => await _service.ScanFileForVirusesAsync(file));
            //Assert
                Assert.Null(exception);
            }
        }
        [Fact]
        public async void ScanFileForVirusesAsync_InfectedFile_ExceptionIsThrown()
        {
            //Arrange
            string text = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
            using (var stream = GenerateStreamFromString(text))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("test-doc-see-as-virus-by-clamav.txt"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/txt"
                };
            //Act
                var exception = await Record.ExceptionAsync(async () => await _service.ScanFileForVirusesAsync(file));
            //Assert
                Assert.NotNull(exception);
                Assert.Equal("The file infected with a virus.", exception.Message);
            }
        }
        [Fact]
        public void CheckFileForAvatar_CorrectSizeAndExtension_Passed()
        {
            //Arrange
            string content = "Hello World from a Fake File";
            using (var stream = GenerateStreamFromString(content))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("testFile.png"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/png"
                };
            //Act
                var exception = Record.Exception(() => _service.CheckFileForAvatar(file));
            //Assert
            Assert.Null(exception);    
            }
        }
        [Fact]
        public void CheckFileForAvatar_FileOwersize_ExceptionIsThrown()
        {
            //Arrange
            int maxSizeForAvatarFile = 2097152;
            var fileMock = new Mock<IFormFile>();
            //Setup mock file using a memory stream
            var content = "Hello World from a Fake File";
            var fileName = "test.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(maxSizeForAvatarFile + 1);
            //Act
            var exception = Record.Exception(() => _service.CheckFileForAvatar(fileMock.Object));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("The file is larger than the server is able or willing to process. The download file must be no more than 2 MB", exception.Message);
        }
        [Fact]
        public void CheckFileForAvatar_FileSizeIsZero_ExceptionIsThrown()
        {
            //Arrange
            var fileMock = new Mock<IFormFile>();
            //Setup mock file using a memory stream
            var content = "Hello World from a Fake File";
            var fileName = "test.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(0);
            //Act
            var exception = Record.Exception(() => _service.CheckFileForAvatar(fileMock.Object));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("File not selected", exception.Message);
        }
        [Fact]
        public void CheckFileForAvatar_NotCorrectExtension_ExceptionIsThrown()
        {
            //Arrange
            string content = "Hello World from a Fake File";
            using (var stream = GenerateStreamFromString(content))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("testFile.txt"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/txt"
                };
            //Act
                var exception = Record.Exception(() => _service.CheckFileForAvatar(file));
            //Assert
                Assert.NotNull(exception);
                Assert.Equal("AVATAR files must have the '.jpg' or '.png' extension.", exception.Message);
            }     
        }
        [Fact]
        public void CheckFileForAvatar_FileIsNull_ExceptionIsThrown()
        {
            //Arrange        
            //Act
            var exception = Record.Exception(() => _service.CheckFileForAvatar(null));
            //Assert
            Assert.NotNull(exception);
            Assert.Equal("File not selected", exception.Message);
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
