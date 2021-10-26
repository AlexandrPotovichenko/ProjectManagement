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
    [Route("api/cards")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ICardService _CardService;
        private readonly IMapper _mapper;

        public CardController(ICardService CardService, IMapper mapper)
        {
            _CardService = CardService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardDto>>> GetCardsAsync()
        {
            var items = await _CardService.GetCardsAsync();

            var result = _mapper.Map<IEnumerable<CardDto>>(items);

            return Ok(result);
        }

        [HttpGet("{cardId}")]
        [Authorize]
        public async Task<ActionResult<CardDto>> GetCardAsync(int cardId)
        {
            Card card  = await _CardService.GetCardAsync(cardId);

            var result = _mapper.Map<CardDto>(card);

            return Ok(result);
        }

        [HttpPost("CreateCard")]
        [Authorize]
        public async Task<ActionResult<CardDto>> CreateCardAsync([FromBody] PostCardDto itemDto)
        {
            var item = await _CardService.CreateCardAsync(itemDto.Name,itemDto.Description,itemDto.ListId);
            var result = _mapper.Map<CardDto>(item);

            return Ok(result);
        }

        
        [HttpPut("AddMemberToCard")]
        [Authorize]
        public async Task<ActionResult<CardMemberDto>> AddMemberToCardAsync([FromBody] PostCardMemberDto itemDto)
        {
            var item = await _CardService.AddMemberToCardAsync(itemDto.UserId, itemDto.CardId, itemDto.Role);
            var result = _mapper.Map<CardMemberDto>(item);

            return Ok(result);
        }

        [HttpPost("RemoveMemberFromCard")]
        [Authorize]
        public async Task<ActionResult> RemoveMemberFromCardAsync(int memberId, int CardId)
        {
            await _CardService.RemoveMemberFromCardAsync(memberId, CardId);

            return Ok();
        }

        [HttpPut("UpdateMembership")]
        [Authorize]
        public async Task<ActionResult> UpdateMembershipAsync([FromBody] UpdateMembershipDto itemDto)
        {
            await _CardService.UpdateMembershipOfMemberOnCardAsync(itemDto.boardId, itemDto.memberId, itemDto.newRole);

            return Ok();
        }

        [HttpPut("AddCommentToCard")]
        [Authorize]
        public async Task<ActionResult> AddCommentToCardAsync(int cardId, string comment)
        {
            await _CardService.AddCommentToCardAsync(cardId, comment);

            return Ok();
        }

        [HttpPut("MoveCardToList")]
        [Authorize]
        public async Task<ActionResult> MoveCardToList(int cardId, int newListId)
        {
            await _CardService.MoveCardToListAsync(cardId, newListId);
            return Ok();
        }

        [HttpDelete("DeleteCard")]
        [Authorize]
        public async Task<ActionResult> DeleteCardAsync(int cardId)
        {
            await _CardService.DeleteCardAsync(cardId);
            return Ok();
        }

        [HttpDelete("api/cards/Comments/{commentId}")]
        [Authorize]
        public async Task<ActionResult> DeleteCommentOnCardAsync(int cardId, int commentId)
        {
            await _CardService.DeleteCommentOnCardAsync(cardId, commentId);

            return Ok();
        }

        
    }
}
