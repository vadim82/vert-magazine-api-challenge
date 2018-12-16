using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MagazineAPI;
using Newtonsoft.Json;

namespace MagazineAPI
{
    public class MagazineClient : IMagazineClient
    {

        private readonly HttpClient http;
        public MagazineClient()
        {
            http = new HttpClient()
            {
                BaseAddress = new Uri("http://magazinestore.azurewebsites.net/api/")
            };
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync(string token)
        {
            return (await GetAsync<ApiResponse<string[]>>($"categories/{token}")).Data;
        }

        public async Task<IEnumerable<Magazine>> GetMagazinesAsync(string token, string category)
        {
            return (await GetAsync<ApiResponse<Magazine[]>>($"magazines/{token}/{category}")).Data;
        }

        public async Task<IEnumerable<Subscriber>> GetSubscribersAsync(string token)
        {
            return (await GetAsync<ApiResponse<Subscriber[]>>($"subscribers/{token}")).Data;
        }

        public async Task<string> GetTokenAsync()
        {
            return (await GetAsync<ApiResponse>("token")).Token;
        }

        public async Task<PostResult> SubmitAnswerAsync(string token, string[] ids)
        {
            var req = JsonConvert.SerializeObject(new { subscribers = ids });
            var content = new StringContent(req, Encoding.UTF8, "application/json");
            var response = await http.PostAsync($"answer/{token}", content);
            string strResult = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PostResult>>(strResult);
            return apiResponse.Data;
        }

        private async Task<T> GetAsync<T>(string path)
        {
            var strResponse = await http.GetStringAsync(path);
            return JsonConvert.DeserializeObject<T>(strResponse);
        }
    }
}