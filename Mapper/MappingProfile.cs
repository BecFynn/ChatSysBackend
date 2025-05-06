using AutoMapper;
using ChatSysBackend.Controllers.Database.DTO;
using ChatSysBackend.Database.Models;


namespace ChatSysBackend.Controllers.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<Groupchat, GroupchatDTO>();
    }
}