using AutoMapper;
using CCSWebKySearch.Dtos;
using CCSWebKySearch.Models;

namespace CCSWebKySearch.Configuration
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<NotebookModel, NotebookDto>().ReverseMap();

            CreateMap<MarriageLicenseModel, MarriageLicenseDto>().ReverseMap();
        }

    }

}