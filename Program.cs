using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MagazineAPI
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new MagazineClient();
            var token = await client.GetTokenAsync();

            var categories = (await client.GetCategoriesAsync(token)).ToList();

            var tSubs = client.GetSubscribersAsync(token);

            var tMags = categories
                .Select(cat => client.GetMagazinesAsync(token, cat))
                .ToList();

            var parallelTasks = new List<Task> { tSubs }.Concat(tMags).ToArray();
            await Task.WhenAll(parallelTasks);

            // hashmap of magezineId -> categoryId
            var magToCatMap = tMags
                .SelectMany(t => t.Result)
                .ToDictionary(t => t.Id, t => t.Category);

            var matchingSubIds = tSubs.Result
                .Where(s => s.MagazineIds.Select(m => magToCatMap[m]).Distinct().Count() == categories.Count)
                .Select(s => s.Id)
                .ToArray();

            var result = await client.SubmitAnswerAsync(token, matchingSubIds);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.None));
        }
    }
}
