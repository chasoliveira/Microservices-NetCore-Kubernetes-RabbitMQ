using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Profiles;

public class PlatformProfile : Profile
{
  public PlatformProfile()
  {
    this.CreateMap<Platform, PlatformReadDto>();
    this.CreateMap<PlatformCreateDto, Platform>();
    this.CreateMap<PlatformReadDto, PlatformPublishedDto>();
    this.CreateMap<Platform, GrpcPlatformModel>()
    .ForMember(dest => dest.PlatformId, opt => opt.MapFrom(src => src.Id));
  }
}