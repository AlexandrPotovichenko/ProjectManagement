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
    [Route("api/boards")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _BoardService;
        private readonly IMapper _mapper;

        public BoardController(IBoardService BoardService, IMapper mapper)
        {
            _BoardService = BoardService;
            _mapper = mapper;
        }

        [HttpGet("GetBoards")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoardsAsync()
        {
            var items = await _BoardService.GetBoardsAsync();
            var result = _mapper.Map<IEnumerable<BoardDto>>(items);

            return Ok(result);
        }

        [HttpGet("GetBoard")]
        [Authorize]
        public async Task<ActionResult<BoardDto>> GetBoardAsync(int boardId)
        {
            Board board = await _BoardService.GetBoardAsync(boardId);
            var result = _mapper.Map<BoardDto>(board);

            return Ok(result);
        }


        [HttpPost("CreateBoard")]
        [Authorize]
        public async Task<ActionResult<BoardDto>> CreateBoardAsync([FromBody] PostBoardDto itemDto)
        {
            var item = await _BoardService.CreateBoardAsync(itemDto.Name,itemDto.Description);
            var result = _mapper.Map<BoardDto>(item);

            return Ok(result);
        }

        [HttpPut("AddMemberToBoard")]
        [Authorize]
        public async Task<ActionResult<BoardMemberDto>> AddMemberToBoardAsync([FromBody] PostBoardMemberDto itemDto)
        {
            var item = await _BoardService.AddMemberToBoardAsync(itemDto.UserId,itemDto.BoardId, itemDto.Role);
            var result = _mapper.Map<BoardMemberDto>(item);

            return Ok(result);
        }

        [HttpPut("UpdateMembership")]
        [Authorize]
        public async Task<ActionResult> UpdateMembershipAsync([FromBody] UpdateMembershipDto itemDto)
        {
            await _BoardService.UpdateMembershipOfMemberOnBoardAsync(itemDto.boardId, itemDto.memberId, itemDto.newRole);
            return Ok();
        }

        [HttpDelete("RemoveMemberFromBoard")]
        [Authorize]
        public async Task<ActionResult> RemoveMemberFromBoardAsync(int memberId,int boardId)
        {
            await _BoardService.RemoveMemberFromBoardAsync(memberId);
            return Ok();
        }

        [HttpDelete("DeleteBoard")]
        [Authorize]
        public async Task<ActionResult> DeleteBoardAsync(int boardId)
        {
            await _BoardService.DeleteBoardAsync(boardId);
            return Ok();
        }

    }
}
