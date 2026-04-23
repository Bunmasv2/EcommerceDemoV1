using AutoMapper;
using EcommerceDemoV1.Domain.Entities;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Khách"));
    }
}