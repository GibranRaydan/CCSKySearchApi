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
            // You can add more complex logic here if needed.
            return true;
        }
    }
}
