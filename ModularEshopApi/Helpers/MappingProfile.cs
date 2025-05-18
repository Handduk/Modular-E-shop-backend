using ModularEshopApi.Models;
using AutoMapper;
using ModularEshopApi.Dto.User;

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
