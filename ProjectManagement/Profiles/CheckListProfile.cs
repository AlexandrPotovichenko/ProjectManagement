using AutoMapper;
using ProjectManagement.Domain.Models;
using ProjectManagement.Dto;

namespace ProjectManagement.Profiles
{
    public class CheckListProfile : Profile
    {
        public CheckListProfile()
        {
            CreateMap<CheckList, CheckListDto>();
            CreateMap<CheckListItem, CheckListItemDto>();
            CreateMap<Board, BoardDto>();
        }
    }
}
