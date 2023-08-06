using AutoMapper;
using backend.Dto;
using backend.Models;

namespace backend.Profiles;

public class UserInfoProfile : Profile
{
    public UserInfoProfile()
    {
        CreateMap<UserInfo, UserInfoDto>()
            .ReverseMap();
    }
}
