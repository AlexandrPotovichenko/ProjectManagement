using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.Domain.Models;
using ProjectManagement.Dto;


namespace ProjectManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly IBoardService _BoardService;
        private readonly IMapper _mapper;

        public BoardsController(IBoardService BoardService, IMapper mapper)
        {
            _BoardService = BoardService;
            _mapper = mapper;
        }
        
        [HttpGet] // GET api/Boards
        [Authorize]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoardsAsync()
        {
            var items = await _BoardService.GetBoardsAsync();
            var result = _mapper.Map<IEnumerable<BoardDto>>(items);
            return Ok(result);
        }
 
        [HttpGet("{boardId}")] // GET api/Boards/123
        [Authorize] 
        public async Task<ActionResult<BoardDto>> GetBoardAsync(int boardId)
        {
            Board board = await _BoardService.GetBoardAsync(boardId);
            var result = _mapper.Map<BoardDto>(board);
            return Ok(result);
        }

        [HttpPost]// POST api/Boards
        [Authorize]
        public async Task<ActionResult<BoardDto>> CreateBoardAsync([FromBody] PostBoardDto itemDto)
        {
            var item = await _BoardService.CreateBoardAsync(itemDto.Name,itemDto.Description);
            var result = _mapper.Map<BoardDto>(item);
            return Created("~api/Boards/"+result.Id, result);
        }

        [HttpPost("{boardId}/members")]// POST api/Boards/123/members
        [Authorize]
        public async Task<ActionResult<BoardMemberDto>> AddMemberToBoardAsync(int boardId, [FromBody] PostBoardMemberDto itemDto)
        {
            var item = await _BoardService.AddMemberToBoardAsync(itemDto.UserId,boardId, itemDto.Role);
            var result = _mapper.Map<BoardMemberDto>(item);
            return Created("~api/Boards/" + boardId+"/members/"+result.Id, result);
        }

        [HttpPut("{boardId}/members/{int memberId}")] // PUT api/Boards/123/members/456
        [Authorize]
        public async Task<ActionResult> UpdateMembershipAsync(int boardId, int memberId, [FromBody] UpdateMembershipDto itemDto)
        {
            await _BoardService.UpdateMembershipOfMemberOnBoardAsync(boardId, memberId, itemDto.newRole);
            return Ok();
        }

        [HttpDelete("{boardId}/members/{memberId}")] // DELETE api/Boards/123/members/456
        [Authorize]
        public async Task<ActionResult> RemoveMemberFromBoardAsync(int boardId, int memberId)
        {
            await _BoardService.RemoveMemberFromBoardAsync(memberId);
            return NoContent();
        }

        [HttpDelete("{boardId}")] // DELETE api/Boards/123
        [Authorize]
        public async Task<ActionResult> DeleteBoardAsync(int boardId)
        {
            await _BoardService.DeleteBoardAsync(boardId);
            return NoContent();
        }
    }
}
