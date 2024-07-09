using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CCSWebKySearch.Models
{
    public class MappingNotebook : Profile
    {
        public MappingNotebook()
        {
            CreateMap<NotebookModel, NotebookDto>();
        }
    }
}
