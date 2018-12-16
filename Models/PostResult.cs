using System.Collections.Generic;

namespace MagazineAPI
{
    public class PostResult
    {
        public string TotalTime { get; set; }
        public bool AnswerCorrect { get; set; }
        public List<string> ShouldBe { get; set; }
    }
}