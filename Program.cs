using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace MagazineAPI
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string apiUri = "http://magazinestore.azurewebsites.net/api";
        private static readonly string apiToken = GetApiToken().Result;
        private static List<string> Categories;
        private static List<Subscriber> Subscribers;
        private static Dictionary<string, List<Magazine>> MagazinesByCategory = new Dictionary<string, List<Magazine>>();

        static void Main(string[] args)
        {
            Categories = GetCategories().Result;
            Subscribers = GetSubscribers().Result;

            foreach (var category in Categories)
            {
                List<Magazine> magazines = GetMagazinesWithCategory(category).Result;
                MagazinesByCategory[category] = magazines;
            }

            List<Subscriber> subscribersToAllCategories = Subscribers.FindAll(SubscribesToAllCategories);
            List<string> subscriberIds = new List<string>();

            foreach (var subscriber in subscribersToAllCategories)
                subscriberIds.Add(subscriber.id);

            PostResult resultObj = SubmitAnswer(subscriberIds).Result;

            Console.WriteLine($"Answer correct: {resultObj.answerCorrect}");
            Console.WriteLine($"Execution time: {resultObj.totalTime}");

        }

        private static bool SubscribesToAllCategories(Subscriber s)
        {
            int numCategories = 0;

            foreach (var category in Categories)
            {
                if (s.magazineIds.Exists((int id) => MagazinesByCategory[category].Exists((Magazine m) => m.id == id)))
                    numCategories++;

            }

            return numCategories == Categories.Count;
        }

        private static async Task<string> GetApiToken()
        {
            string response = await httpClient.GetStringAsync($"{apiUri}/token");
            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            return data["token"];
        }

        private static async Task<List<string>> GetCategories()
        {
            string response = await httpClient.GetStringAsync($"{apiUri}/categories/{apiToken}");
            CategoriesResponseData data = BuildCategoriesResponseData(response);
            return data.data;
        }

        private static async Task<List<Magazine>> GetMagazinesWithCategory(string category)
        {
            string response = await httpClient.GetStringAsync($"{apiUri}/magazines/{apiToken}/{category}");
            MagazinesResponseData data = BuildMagazinesResponseData(response);
            return data.data;
        }

        private static async Task<List<Subscriber>> GetSubscribers()
        {
            string response = await httpClient.GetStringAsync($"{apiUri}/subscribers/{apiToken}");
            SubscribersResponseData data = BuildSubscribersResponseData(response);
            return data.data;
        }

        private static async Task<PostResult> SubmitAnswer(List<string> subscriberIds)
        {
            Dictionary<string, List<string>> requestData = new Dictionary<string, List<string>>();
            requestData["subscribers"] = subscriberIds;
            string json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync($"{apiUri}/answer/{apiToken}", content);
            string result = await response.Content.ReadAsStringAsync();
            PostResultData data = BuildPostResultData(result);
            return data.data;
        }

        private static CategoriesResponseData BuildCategoriesResponseData(string json) => JsonConvert.DeserializeObject<CategoriesResponseData>(json);

        private static MagazinesResponseData BuildMagazinesResponseData(string json) => JsonConvert.DeserializeObject<MagazinesResponseData>(json);

        private static SubscribersResponseData BuildSubscribersResponseData(string json) => JsonConvert.DeserializeObject<SubscribersResponseData>(json);

        private static PostResultData BuildPostResultData(string json) => JsonConvert.DeserializeObject<PostResultData>(json);

        private class CategoriesResponseData
        {
            public List<string> data { get; set; }
            public bool success { get; set; }
            public string token { get; set; }
        }

        private class MagazinesResponseData
        {
            public List<Magazine> data { get; set; }
            public bool success { get; set; }
            public string token { get; set; }
        }

        private class SubscribersResponseData
        {
            public List<Subscriber> data { get; set; }
            public bool success { get; set; }
            public string token { get; set; }
        }

        private class PostResultData
        {
            public PostResult data { get; set; }
            public bool success { get; set; }
            public string token { get; set; }
            public string message { get; set; }
        }

        private class Magazine
        {
            public int id { get; set; }
            public string name { get; set; }
            public string category { get; set; }
        }

        private class Subscriber
        {
            public string id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public List<int> magazineIds { get; set; }
        }

        private class PostResult
        {
            public string totalTime { get; set; }
            public bool answerCorrect { get; set; }
            public List<string> shouldBe { get; set; }
        }
    }
}
