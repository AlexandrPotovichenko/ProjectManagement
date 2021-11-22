using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Options;
using ProjectManagement.BusinessLogic.Services.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ProjectManagement.BusinessLogic.Tests.Service
{
    public class FileServiceTests
    {
        private readonly Mock<ILogger<FileService>> _logger;
        private readonly ClamAVServerOptions _options;
        private readonly FileService _service;
        public FileServiceTests()
        {
            _logger = new Mock<ILogger<FileService>>();
            _options = Mock.Of<ClamAVServerOptions>(o => o.URL == "127.0.0.1" && o.Port == "3310");
            _service = new FileService(_logger.Object, _options);
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
        //[Fact]
        //public async void CheckFileForAvatar_NotInfectedFile_Passed()
        //{
        //    if (file == null || file.Length == 0)
        //        throw new WebAppException((int)HttpStatusCode.UnprocessableEntity, "file not selected");
        //    // Upload the file if less than 2 MB
        //    if (file.Length > _maxSizeForAvatarFile)
        //    {
        //    string extension = Path.GetExtension(file.FileName); extension != ".jpg" && extension != ".png")

        //    //Arrange
        //    string text = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
        //    using (var stream = GenerateStreamFromString(text))
        //    {
        //        var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("test-doc-see-as-virus-by-clamav.txt"))
        //        {
        //            Headers = new HeaderDictionary(),
        //            ContentType = "application/txt"
        //        };
        //        //Act
        //        var exception = await Record.ExceptionAsync(async () => await _service.ScanFileForVirusesAsync(file));
        //        //Assert
        //        Assert.NotNull(exception);
        //        Assert.Equal("The file infected with a virus.", exception.Message);


        //    }
        //}
        //Task ScanFileForVirusesAsync(IFormFile file)
        //    void CheckFileForAvatar(IFormFile file)

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
