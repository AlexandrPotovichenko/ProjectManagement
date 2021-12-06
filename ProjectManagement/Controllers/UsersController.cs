
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]// POST api/Users
        [Authorize]
        public async Task<ActionResult> CreateUserAsync([FromBody] PostUserDto itemDto)
        {
            User user = await _userService.RegisterUserAsync(itemDto.Name, itemDto.Password);
            GetUserDto getUserDto = _mapper.Map<GetUserDto>(user);
            return Created("~api/Users/" + user.Id, getUserDto);
        }

        [HttpPut("{userId}")] // PUT api/Users/123
        [Authorize]
        public async Task<ActionResult> UpdateUserAsync(int userId, [FromBody] PostUserDto itemDto)
        {
            await _userService.UpdateUserAsync(userId, itemDto.Name, itemDto.Password);
            return Ok();
        }

        [HttpPost("{userId}/avatar")] // POST api/Users/123/avatar
        public async Task<ActionResult> UploadAvatarAsync(int userId,IFormFile file)
        {
            await _userService.UploadAvatarAsync(userId,file);
            return Ok("The file is successfully uploaded.");
        }

        [HttpGet("{userId}/avatar")] // GET api/Users/123/avatar
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
