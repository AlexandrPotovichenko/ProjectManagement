﻿using AutoMapper;
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
    [Route("api/checkLists")]
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

        [HttpPost("CreateCheckList")]
        [Authorize]
        public async Task<ActionResult<CheckListDto>> CreateCheckListAsync([FromBody] PostCheckListDto itemDto)
        {
            var item = await _CheckListService.CreateCheckListAsync(itemDto.CardId, itemDto.Name);
            var result = _mapper.Map<CheckListDto>(item);

            return Ok(result);
        }
        [HttpPost("CreateCheckListItem")]
        [Authorize]
        public async Task<ActionResult<CheckListItemDto>> CreateCheckListItemAsync([FromBody] PostCheckListItemDto itemDto)
        {
            var item = await _CheckListService.AddCheckListItemToCheckListAsync(itemDto.CheckListId, itemDto.Name);
            var result = _mapper.Map<CheckListItemDto>(item);

            return Ok(result);
        }


        [HttpGet("api/checkLists/{cardId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CheckListDto>>> CheckListsAsync(int cardId)
        {
            var items = await _CheckListService.GetCheckListsByCardIdAsync(cardId);

            var result = _mapper.Map<IEnumerable<CheckListDto>>(items);

            return Ok(result);
        }

        [HttpGet("api/checkLists/CheckListItems/{checkListId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CheckListItemDto>>> CheckListItemsAsync(int checkListId)
        {
            var items = await _CheckListService.GetCheckListItemsAsync(checkListId);

            var result = _mapper.Map<IEnumerable<CheckListItemDto>>(items);

            return Ok(result);
        }

        [HttpPut("CompleteCheckListItem")]
        [Authorize]
        public async Task<ActionResult> CompleteCheckListItemAsync(int checkListItemId)
        {
            await _CheckListService.CompleteCheckListItemAsync(checkListItemId);
            return Ok();
        }
        [HttpDelete("DeleteCheckList")]
        [Authorize]
        public async Task<ActionResult> DeleteCheckListAsync(int checkListId)
        {
            await _CheckListService.DeleteCheckListAsync(checkListId);

            return Ok();
        }
        [HttpDelete("DeleteCheckListItem")]
        [Authorize]
        public async Task<ActionResult> DeleteCheckListItemAsync( int checkListItemId)
        {
            await _CheckListService.DeleteCheckListItemAsync(checkListItemId);

            return Ok();
        }

    }
}
