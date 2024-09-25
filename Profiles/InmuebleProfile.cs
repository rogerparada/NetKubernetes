using AutoMapper;
using NetKubernetes.Dto.InmueblesDtos;
using NetKubernetes.Models;

namespace NetKubernetes.Profiles;

public class InmuebleProfile : Profile
{
    public InmuebleProfile()
    {
        CreateMap<Inmueble, InmuebleResponseDto>();
        CreateMap<InmuebleRequestDto, Inmueble>();
    }
}