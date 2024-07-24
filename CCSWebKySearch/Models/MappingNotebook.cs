using AutoMapper;

namespace CCSWebKySearch.Models
{
    public class MappingNotebook : Profile
    {
        public MappingNotebook()
        {
            CreateMap<NotebookModel, NotebookDto>();
        }

    }

    public class MappingMarriageLicense : Profile
    {
        public MappingMarriageLicense() 
        {
            CreateMap<MarriageLicenseModel, MarriageLicenseDto>();
        }
    }
}