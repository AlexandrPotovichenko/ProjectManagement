
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.Domain.Models;
using ProjectManagement.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ProjectManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult> UploadAvatarAsync(int userId,IFormFile file)
        {
            await _userService.UploadAvatarAsync(userId,file);
            return Ok("The file is successfully uploaded.");
        }

        [HttpGet]
        public async Task<ActionResult> DownloadAvatarAsync(int userId)
        {
            AppFile avatar = await _userService.DownloadAvatarAsync(userId);
            
            if (avatar!=null)
            {
                return File(new MemoryStream(avatar.Content), avatar.ContentType,avatar.Name);
            }
            else
            {
                return NotFound();
            }    
        }
    }
}
