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



namespace ProjectManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckListsController : ControllerBase
    {
        private readonly ICheckListService _CheckListService;
        private readonly IMapper _mapper;
        public CheckListsController(ICheckListService CheckListService, IMapper mapper)
        {
            _CheckListService = CheckListService;
            _mapper = mapper;
        }

        [HttpPost("Create")]// POST api/CheckLists/Create
        [Authorize]
        public async Task<ActionResult<CheckListDto>> CreateCheckListAsync([FromBody] PostCheckListDto itemDto)
        {
            var item = await _CheckListService.CreateCheckListAsync(itemDto.CardId, itemDto.Name);
            var result = _mapper.Map<CheckListDto>(item);
            return Created("~api/CheckLists/" + result.Id, result);
        }
        [HttpPost("{checkListId}/CreateCheckListItem")]// POST api/CheckLists/123/CreateCheckListItem
        [Authorize]
        public async Task<ActionResult<CheckListItemDto>> CreateCheckListItemAsync(int checkListId,[FromBody] PostCheckListItemDto itemDto)
        {
            var item = await _CheckListService.AddCheckListItemToCheckListAsync(checkListId, itemDto.Name);
            var result = _mapper.Map<CheckListItemDto>(item);
            return Created("~api/CheckLists/" + checkListId+ "/CheckListItems/" + result.Id, result);

        }
        //
        //
        // 
        //[HttpGet("{cardId}")] // GET api/CheckLists/123
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<CheckListDto>>> CheckListsAsync(int cardId)
        //{
        //    var items = await _CheckListService.GetCheckListsByCardIdAsync(cardId);
        //    var result = _mapper.Map<IEnumerable<CheckListDto>>(items);
        //    return Ok(result);
        //}

        [HttpGet("{checkListId}/CheckListItems")] // GET api/CheckLists/123/CheckListItems
        [Authorize]
        public async Task<ActionResult<IEnumerable<CheckListItemDto>>> CheckListItemsAsync(int checkListId)
        {
            var items = await _CheckListService.GetCheckListItemsAsync(checkListId);
            var result = _mapper.Map<IEnumerable<CheckListItemDto>>(items);
            return Ok(result);
        }

        [HttpPut("{checkListId}/CheckListItems/{checkListItemId}")] // PUT api/CheckLists/123/CheckListItems/456
        [Authorize]
        public async Task<ActionResult> CompleteCheckListItemAsync(int checkListId,int checkListItemId)
        {
            await _CheckListService.CompleteCheckListItemAsync(checkListId,checkListItemId);
            return Ok();
        }
        [HttpDelete("{checkListId}")]
        [Authorize]
        public async Task<ActionResult> DeleteCheckListAsync( int checkListId)
        {
            await _CheckListService.DeleteCheckListAsync(checkListId);
            return NoContent();
        }
        [HttpDelete("{checkListId}/CheckListItems/{checkListItemId}")] // DELETE api/CheckLists/123/CheckListItems/456
        [Authorize]
        public async Task<ActionResult> DeleteCheckListItemAsync(int checkListId, int checkListItemId)
        {
            await _CheckListService.DeleteCheckListItemAsync(checkListId, checkListItemId);
            return NoContent();
        }

    }
}
