using ModularEshopApi.Dto;
using ModularEshopApi.Models;
using AutoMapper;

namespace ModularEshopApi.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
        }
    }
}
