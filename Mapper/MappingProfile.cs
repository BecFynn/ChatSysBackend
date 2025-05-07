using AutoMapper;

using ChatSysBackend.Database.Models;


namespace ChatSysBackend.Controllers.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.UserGroupchats,
                opt => opt.MapFrom(src => src.UserGroupchats));

        CreateMap<Groupchat, GroupchatDTO>()
            .ForMember(dest => dest.Users,
                opt => opt.MapFrom(src => src.Users));

        // Short DTOs for nesting
        CreateMap<Groupchat, GroupchatDTO_Short>();
        CreateMap<User, UserDTO_Short>();
        
        CreateMap<Message, MessageDTO>();


    }
}