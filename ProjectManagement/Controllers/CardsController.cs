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
            var items = await _CardService.GetCardsAsync();
            var result = _mapper.Map<IEnumerable<CardDto>>(items);
            return Ok(result);
        }

        [HttpGet("{cardId}")] // GET api/Cards/123
        [Authorize]
        public async Task<ActionResult<CardDto>> GetCardAsync(int cardId)
        {
            Card card  = await _CardService.GetCardAsync(cardId);
            var result = _mapper.Map<CardDto>(card);
            return Ok(result);
        }

        [HttpPost("Create")] // POST api/Cards/Create
        [Authorize]
        public async Task<ActionResult<CardDto>> CreateCardAsync([FromBody] PostCardDto itemDto)
        {
            var item = await _CardService.CreateCardAsync(itemDto.Name,itemDto.Description,itemDto.ListId);
            var result = _mapper.Map<CardDto>(item);
            return Created("~api/Cards/" + result.Id, result);
        }

        [HttpPut("{cardId}/members/AddMember")]// POST api/Cards/123/members/AddMember
        [Authorize]
        public async Task<ActionResult<CardMemberDto>> AddMemberToCardAsync(int cardId,[FromBody] PostCardMemberDto itemDto)
        {
            var item = await _CardService.AddMemberToCardAsync(itemDto.UserId, cardId, itemDto.Role);
            var result = _mapper.Map<CardMemberDto>(item);
            return Created("~api/Cards/" + cardId + "/members/" + result.Id, result);
        }

        [HttpPut("{cardId}/members/UpdateMember")] // PUT api/Cards/123/members/UpdateMember
        [Authorize]
        public async Task<ActionResult> UpdateMembershipAsync(int cardId,[FromBody] UpdateMembershipDto itemDto)
        {
            await _CardService.UpdateMembershipOfMemberOnCardAsync(cardId, itemDto.memberId, itemDto.newRole);
            return Ok();
        }

        [HttpDelete("{cardId}/members/{memberId}")] // DELETE api/Cards/123/members/456
        [Authorize]
        public async Task<ActionResult> RemoveMemberFromCardAsync(int cardId, int memberId)
        {
            await _CardService.RemoveMemberFromCardAsync(memberId, cardId);
            return NoContent();
        }

        [HttpPut("{cardId}/Comments/AddComment")] // PUT api/Cards/123/Comments/AddComment
        [Authorize]
        public async Task<ActionResult> AddCommentToCardAsync(int cardId, string comment)
        {
            var item = await _CardService.AddCommentToCardAsync(cardId, comment);
            return Created("~api/Cards/" + cardId + "/Comments/" + item.Id, item);
        }

        [HttpGet("{cardId}/Comments")]  // GET api/Cards/123/Comments
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardAction>>> GetCommentsAsync(int cardId)
        {
            var items = await _CardService.GetCommentsOnCardAsync(cardId);
            var result = _mapper.Map<IEnumerable<CardAction>>(items);
            return Ok(result);
        }

        [HttpGet("{cardId}/Comments/{commentId}")]  // GET api/Cards/123/Comments
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardAction>>> GetCommentAsync(int cardId,int commentId)
        {
            var item = await _CardService.GetCommentOnCardAsync(cardId, commentId);
            var result = _mapper.Map<CardAction>(item);
            return Ok(result);
        }

        [HttpPut("{cardId}/MoveToList")] // PUT api/Cards/123/MoveToList
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
