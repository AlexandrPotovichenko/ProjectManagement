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
    [Route("api/check_lists")]
    [ApiController]
    public class CheckListController : ControllerBase
    {
        private readonly ICheckListService _CheckListService;
        private readonly IMapper _mapper;

        public CheckListController(ICheckListService CheckListService, IMapper mapper)
        {
            _CheckListService = CheckListService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CheckListItemDto>>> GetItemsAsync()
        {
            var items = await _CheckListService.GetCheckListItemsAsync(UserId);

            var result = _mapper.Map<IEnumerable<CheckListItemDto>>(items);

            return Ok(result);
        }

        //[HttpGet("completed")]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<GetCheckListItemDto>>> GetCompletedItemsAsync()
        //{
        //    var items = await _toCheckListItemService.GetCompletedItemsAsync(UserId);

        //    var result = _mapper.Map<IEnumerable<GetCheckListItemDto>>(items);

        //    return Ok(result);
        //}

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CheckListItemDto>> CreateCheckListItemAsync([FromBody] CheckListItemDto itemDto)
        {
            var item = await _CheckListService.AddCheckListItemAsync(itemDto.CheckListId, itemDto.Name);
            var result = _mapper.Map<CheckListItemDto>(item);

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
