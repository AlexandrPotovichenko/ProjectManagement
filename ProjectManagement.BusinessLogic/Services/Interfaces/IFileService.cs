using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface IFileService
    {
        Task ScanFileForVirusesAsync(IFormFile file);
        void CheckFileForAvatar(IFormFile file);
    }
}
