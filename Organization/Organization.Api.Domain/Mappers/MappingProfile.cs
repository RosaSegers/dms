using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organization.Api.Domain.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Entities.Invite, Dtos.Invite>();
            CreateMap<Entities.Member, Dtos.Member>();
            CreateMap<Entities.Organization, Dtos.Organization>();
        }
    }
}
