using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagazineAPI
{
    public interface IMagazineClient
    {
        Task<string> GetTokenAsync();
        Task<IEnumerable<string>> GetCategoriesAsync(string token);
        Task<IEnumerable<Subscriber>> GetSubscribersAsync(string token);
        Task<IEnumerable<Magazine>> GetMagazinesAsync(string token, string category);

        Task<PostResult> SubmitAnswerAsync(string token, string[] ids);
    }
}