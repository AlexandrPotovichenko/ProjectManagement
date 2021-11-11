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
    public class CardsController : ControllerBase
    {
        private readonly ICardService _CardService;
        private readonly IMapper _mapper;
        public CardsController(ICardService CardService, IMapper mapper)
        {
            _CardService = CardService;
            _mapper = mapper;
        }

        [HttpGet]  // GET api/Cards
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardDto>>> GetCardsAsync()
        {
            IEnumerable<Card> cards = await _CardService.GetCardsAsync();
            IEnumerable<CardDto> result = _mapper.Map<IEnumerable<CardDto>>(cards);
            return Ok(result);
        }

        [HttpGet("{cardId}")] // GET api/Cards/123
        [Authorize]
        public async Task<ActionResult<CardDto>> GetCardAsync(int cardId)
        {
            Card card  = await _CardService.GetCardAsync(cardId);
            CardDto result = _mapper.Map<CardDto>(card);
            return Ok(result);
        }

        [HttpPost] // POST api/Cards
        [Authorize]
        public async Task<ActionResult> CreateCardAsync([FromBody] PostCardDto itemDto)
        {
            Card card = await _CardService.CreateCardAsync(itemDto.Name,itemDto.Description,itemDto.ListId);
            CardDto cardDto = _mapper.Map<CardDto>(card);
            return Created("~api/Cards/" + card.Id, cardDto);
        }

        [HttpPost("{cardId}/members")]// POST api/Cards/123/members
        [Authorize]
        public async Task<ActionResult> AddMemberToCardAsync(int cardId,[FromBody] PostCardMemberDto itemDto)
        {
            CardMember cardMember = await _CardService.AddMemberToCardAsync(itemDto.UserId, cardId, itemDto.Role);
            CardMemberDto cardMemberDto  = _mapper.Map<CardMemberDto>(cardMember);
            return Created("~api/Cards/" + cardId + "/members/" + cardMember.Id, cardMemberDto);
        }

        [HttpPut("{cardId}/members/{memberId}")] // PUT api/Cards/123/members/456
        [Authorize]
        public async Task<ActionResult> UpdateMembershipAsync(int cardId,int memberId,[FromBody] UpdateMembershipDto itemDto)
        {
            await _CardService.UpdateMembershipOfMemberOnCardAsync(cardId, memberId, itemDto.newRole);
            return Ok();
        }

        [HttpDelete("{cardId}/members/{memberId}")] // DELETE api/Cards/123/members/456
        [Authorize]
        public async Task<ActionResult> RemoveMemberFromCardAsync(int cardId, int memberId)
        {
            await _CardService.RemoveMemberFromCardAsync(memberId, cardId);
            return NoContent();
        }

        [HttpPost("{cardId}/Comments")] // POST api/Cards/123/Comments/AddComment
        [Authorize]
        public async Task<ActionResult> AddCommentToCardAsync(int cardId, string comment)
        {
            CardAction cardAction = await _CardService.AddCommentToCardAsync(cardId, comment);
            CardActionDto cardActionDto = _mapper.Map<CardActionDto>(cardAction);
            return Created("~api/Cards/" + cardId + "/Comments/" + cardAction.Id, cardActionDto);
        }

        [HttpGet("{cardId}/Comments")]  // GET api/Cards/123/Comments
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardActionDto>>> GetCommentsAsync(int cardId)
        {
            IEnumerable<CardAction> cardActions = await _CardService.GetCommentsOnCardAsync(cardId);
            IEnumerable<CardActionDto> result = _mapper.Map<IEnumerable<CardActionDto>>(cardActions);
            return Ok(result);
        }

        [HttpGet("{cardId}/Comments/{commentId}")]  // GET api/Cards/123/Comments
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardActionDto>>> GetCommentsAsync(int cardId,int commentId)
        {
            IEnumerable<CardAction> item = await _CardService.GetCommentsOnCardAsync(cardId, commentId);
            IEnumerable<CardActionDto> result = _mapper.Map<IEnumerable<CardActionDto>>(item);
            return Ok(result);
        }

        [HttpPut("{cardId}/list")] // PUT api/Cards/123/List
        [Authorize]
        public async Task<ActionResult> MoveCardToList(int cardId, int newListId)
        {
            await _CardService.MoveCardToListAsync(cardId, newListId);
            return Ok();
        }

        [HttpDelete("{cardId}")] // DELETE api/Cards/123
        [Authorize]
        public async Task<ActionResult> DeleteCardAsync(int cardId)
        {
            await _CardService.DeleteCardAsync(cardId);
            return NoContent();
        }

        [HttpDelete("{cardId}/Comments/{commentId}")] // DELETE api/Cards/123/Comments/456
        [Authorize]
        public async Task<ActionResult> DeleteCommentOnCardAsync(int cardId, int commentId)
        {
            await _CardService.DeleteCommentOnCardAsync(cardId, commentId);
            return NoContent();
        }
    }
}
