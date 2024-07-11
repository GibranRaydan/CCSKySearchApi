namespace CCSWebKySearch.Services
{
    public interface ICheckLiveService
    {
        bool IsLive();
    }

    public class CheckLiveService : ICheckLiveService
    {
        public bool IsLive()
        {
            return true;
        }
    }
}
