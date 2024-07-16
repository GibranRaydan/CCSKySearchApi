using System.Collections.Generic;
using System.Threading.Tasks;
using CCSWebKySearch.Models;

namespace CCSWebKySearch.Services
{
    public interface INotebookService
    {
        Task<IEnumerable<NotebookModel>> GetAllNotebooksAsync(int count);
    }

    public interface ILandSearchPageBookService
    {
        Task<IEnumerable<NotebookModel>> SearchPageBookService(int book, int page);
    }

    public interface IKindSearchService
    {
        Task<IEnumerable<NotebookModel>> SearchByKindsAsync(List<string> kinds);
    }

    public interface IDocumentFileService
    {
        Task<byte[]> GetDocumentFileAsync(string book, string page, string fileType);
    }
}
