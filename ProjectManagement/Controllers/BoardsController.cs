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
            IEnumerable<Board> boards = await _BoardService.GetBoardsAsync();
            IEnumerable<BoardDto> boardsDto = _mapper.Map<IEnumerable<BoardDto>>(boards);
            return Ok(boardsDto);
        }
 
        [HttpGet("{boardId}")] // GET api/Boards/123
        [Authorize] 
        public async Task<ActionResult<BoardDto>> GetBoardAsync(int boardId)
        {
            Board board = await _BoardService.GetBoardAsync(boardId);
            BoardDto boardDto = _mapper.Map<BoardDto>(board);
            return Ok(boardDto);
        }

        [HttpPost]// POST api/Boards
        [Authorize]
        public async Task<ActionResult> CreateBoardAsync([FromBody] PostBoardDto itemDto)
        {
            Board board = await _BoardService.CreateBoardAsync(itemDto.Name,itemDto.Description);
            BoardDto boardDto = _mapper.Map<BoardDto>(board);
            return Created("~api/Boards/"+ board.Id, boardDto);
        }

        [HttpPost("{boardId}/members")]// POST api/Boards/123/members
        [Authorize]
        public async Task<ActionResult> AddMemberToBoardAsync(int boardId, [FromBody] PostBoardMemberDto itemDto)
        {
            BoardMember boardMember = await _BoardService.AddMemberToBoardAsync(itemDto.UserId,boardId, itemDto.Role);
            BoardMemberDto boardMemberDto = _mapper.Map<BoardMemberDto>(boardMember);
            return Created("~api/Boards/" + boardId+"/members/"+ boardMember.Id, boardMember);
        }

        [HttpPut("{boardId}/members/{memberId}")] // PUT api/Boards/123/members/456
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
