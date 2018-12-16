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

            // Gets a list of Tasks for each category.
            var tMags = categories
                .Select(cat => client.GetMagazinesAsync(token, cat))
                .ToList();
            // take the subs task and all the magazine tasks and wait for all of them to complete
            var parallelTasks = new List<Task> { tSubs }.Concat(tMags).ToArray();
            await Task.WhenAll(parallelTasks);

            // hashmap of magezineId -> categoryId
            var magToCatMap = tMags
                .SelectMany(t => t.Result) // Select many is to flatten an array of arrays. [ [1,2], [2,3], [5,6]  ] => [1,2,2,3,5,6]. Similar to calling .flatten().map()
                .ToDictionary(t => t.Id, t => t.Category); // converts a list to a dictionary. First lambda is the key code block, second lambda is the value code block

            // give me all the subscribers where the persons distinct category count is the same as the number of categories.
            var matchingSubIds = tSubs.Result
                .Where(s => s.MagazineIds.Select(m => magToCatMap[m]).Distinct().Count() == categories.Count)
                .Select(s => s.Id)
                .ToArray();

            var result = await client.SubmitAnswerAsync(token, matchingSubIds);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.None));
        }
    }
}
