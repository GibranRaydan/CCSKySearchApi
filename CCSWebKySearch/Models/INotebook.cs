using System.Collections.Generic;
using System.Threading.Tasks;
using CCSWebKySearch.Models;

namespace CCSWebKySearch.Services
{
    public interface INotebookService
    {
        Task<NotebookResponse> GetAllNotebooksAsync(int count);
    }

    public interface ILandSearchPageBookService
    {
        Task<IEnumerable<NotebookModel>> SearchPageBookService(int book, int page);
    }

    public interface IKindSearchService
    {
        Task<IEnumerable<NotebookModel>> SearchByKindsAsync(List<string> kinds);
    }
}
