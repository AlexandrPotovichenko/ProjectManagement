﻿using AutoMapper;
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
            CreateMap<Card, CardDto>().ForMember(dest => dest.ListName, act => act.MapFrom(src => src.List.Name)) ; 
            CreateMap<Card, CardOnListDto>(); 
            CreateMap<CardAction, CardActionDto>();
            CreateMap<CardAction, CardActionOnCardDto>().ForMember(dest => dest.MemberId, act => act.MapFrom(src => src.CardMember.Id)); ;
            CreateMap<CheckList, CheckListDto>();
            CreateMap<CheckList, CheckListOnCardDto>();
            CreateMap<CheckListItem, CheckListItemDto>();
            CreateMap<User, GetUserDto>();
            CreateMap<BoardMember, BoardMemberDto>();
            CreateMap<CardMember, CardMemberDto>();
        }
    }
}
