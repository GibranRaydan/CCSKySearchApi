using System.Collections.Generic;
using System.Threading.Tasks;
using CCSWebKySearch.Models;

namespace CCSWebKySearch.Services
{
    public interface INotebookService
    {
        Task<IEnumerable<NotebookModel>> GetAllNotebooksAsync();
    }
}
