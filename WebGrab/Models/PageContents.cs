using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGrab.Models
{
    public class PageContents
    {
        public HashSet<string> ImageUrls { get; set; }
        public int WordCount { get; set; }
        public List<KeyValuePair<string, int>> TopTen { get; set; }

        public PageContents()
        {
            ImageUrls = new HashSet<string>();
            TopTen = new List<KeyValuePair<string, int>>();
        }
    }
}
