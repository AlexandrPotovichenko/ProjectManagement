using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using nClam;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _configuration;
   
        public FileService(ILogger<FileService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public async Task ScanFileForVirusesAsync(IFormFile file)
        {
            bool? isFileInfected = await IsFileInfectedAsync(file);
            if(isFileInfected==null)
            {
                string message = $"The file may be infected with a virus.";
                this._logger.LogError(message);
                throw new WebAppException((int)HttpStatusCode.BadGateway, message);
            }
            else if (isFileInfected == true)
            {
                string message = $"The file infected with a virus.";
                this._logger.LogError(message);
                throw new WebAppException((int)HttpStatusCode.BadGateway, message);
            }
        }

        public async Task<bool?> IsFileInfectedAsync(IFormFile file)
        {
            var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);
            byte[] fileBytes = ms.ToArray();
            try
            {
                this._logger.LogInformation("ClamAV scan begin for file {0}", file.FileName);
                var clam = new ClamClient(this._configuration["ClamAVServer:URL"],
                                          Convert.ToInt32(this._configuration["ClamAVServer:Port"]));
                var scanResult = await clam.SendAndScanFileAsync(fileBytes);
                switch (scanResult.Result)
                {
                    case ClamScanResults.Clean:
                        this._logger.LogInformation("The file is clean! ScanResult:{1}", scanResult.RawResult);
                        return false;
                    case ClamScanResults.VirusDetected:
                        this._logger.LogError("Virus Found! Virus name: {1}", scanResult.InfectedFiles.FirstOrDefault().VirusName);
                        return true;
                    case ClamScanResults.Error:
                        this._logger.LogError("An error occured while scaning the file! ScanResult: {1}", scanResult.RawResult);
                        break;
                    case ClamScanResults.Unknown:
                        this._logger.LogError("Unknown scan result while scaning the file! ScanResult: {0}", scanResult.RawResult);
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = $"ClamAV Scan Exception: {ex.ToString()}";
                this._logger.LogError("ClamAV Scan Exception: {0}", ex.ToString());
                throw new WebAppException((int)HttpStatusCode.BadGateway, message);
            }
            this._logger.LogInformation("ClamAV scan completed for file {0}", file.FileName);
            return null;
        }
        public void CheckFileForAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new WebAppException((int)HttpStatusCode.UnprocessableEntity, "file not selected");
            // Upload the file if less than 2 MB
            if (file.Length > 2097152)
            {
                throw new WebAppException((int)HttpStatusCode.RequestEntityTooLarge, "The file is larger than the server is able or willing to process. The download file must be no more than 2 MB");
            }
            string extension = Path.GetExtension(file.FileName);
            if (extension != ".jpg" && extension != ".png")
            {
                throw new WebAppException((int)HttpStatusCode.UnsupportedMediaType, "AVATAR files must have the '.jpg' or '.png' extension.");
            }
        }
    }
}
