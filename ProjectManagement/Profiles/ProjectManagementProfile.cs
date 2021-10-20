using AutoMapper;
using ProjectManagement.Domain.Models;
using ProjectManagement.Dto;

namespace ProjectManagement.Profiles
{
    public class ProjectManagementProfile : Profile
    {
        public ProjectManagementProfile()
        {
            CreateMap<Board, BoardDto>();
            CreateMap<List, ListDto>();
            CreateMap<Card, CardDto>();
            CreateMap<CardAction, CardActionDto>();
            CreateMap<CheckList, CheckListDto>();
            CreateMap<CheckListItem, CheckListItemDto>();
            
            
           
            
            CreateMap<BoardMember, BoardMemberDto>();
            CreateMap<CardMember, CardMemberDto>();
            //.ReverseMap().ForMember(a => a.Lists, a => a.Ignore()); 
        }
    }
}
