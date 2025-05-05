using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Domain.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Entities.Role, Dtos.Role>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions));

            CreateMap<Entities.Permission, Dtos.Permission>();

            CreateMap<ICollection<Entities.Permission>, List<Dtos.Permission>>()
                .ConvertUsing(src => src.Select(x => new Dtos.Permission
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList());
        }
    }
}
