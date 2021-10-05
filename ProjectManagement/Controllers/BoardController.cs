using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.Domain.Models;
using ProjectManagement.Dto;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoardsAsync()
        {
            var items = await _BoardService.GetBoardsAsync();

            var result = _mapper.Map<IEnumerable<BoardDto>>(items);

            return Ok(result);
        }

        //[HttpGet("completed")]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<GetBoardDto>>> GetCompletedItemsAsync()
        //{
        //    var items = await _toBoardService.GetCompletedItemsAsync(UserId);

        //    var result = _mapper.Map<IEnumerable<GetBoardDto>>(items);

        //    return Ok(result);
        //}

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BoardDto>> CreateBoardAsync([FromBody] PostBoardDto itemDto)
        {
            var item = await _BoardService.CreateBoardAsync(itemDto.Name,itemDto.Description, UserId);
            var result = _mapper.Map<BoardDto>(item);

            return Ok(result);
        }

        //[HttpPut("{id}")]
        //[Authorize]
        //public async Task<ActionResult> CompleteItemAsync(int id)
        //{
        //    await _toDoItemService.CompleteToDoItemAsync(UserId, id);

        //    return Ok();
        //}


        private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
}
