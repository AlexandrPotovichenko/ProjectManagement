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

        //[HttpGet("completed")]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<GetCardDto>>> GetCompletedItemsAsync()
        //{
        //    var items = await _toCardService.GetCompletedItemsAsync(UserId);

        //    var result = _mapper.Map<IEnumerable<GetCardDto>>(items);

        //    return Ok(result);
        //}

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CardDto>> CreateCardAsync([FromBody] PostCardDto itemDto)
        {
            var item = await _CardService.CreateCardAsync(itemDto.Name,itemDto.Description,itemDto.ListId, UserId);
            var result = _mapper.Map<CardDto>(item);

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
