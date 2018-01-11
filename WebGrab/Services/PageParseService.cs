using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebGrab.Models;

namespace WebGrab.Services
{
    public class PageParseService
    {
        public async Task<PageContents> ParseAsync(string url)
        {
            // instance or static variable
            HttpClient client = new HttpClient();
            PageContents pageContents = new PageContents();

            try
            {
                // get answer in non-blocking way
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        // get base address of url for images defined relatively
                        Uri uri = new Uri(url);
                        // location after domain
                        int y = uri.AbsoluteUri.LastIndexOf(uri.PathAndQuery);
                        string baseAddress = uri.AbsoluteUri.Substring(0, y);

                        // read answer in non-blocking way
                        var result = await content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(result);
                        var nodes = document.DocumentNode.SelectNodes("//img[@src]");
                        foreach (HtmlNode img in nodes)
                        {
                            HtmlAttribute srcAttribute = img.Attributes["src"];
                            string src = srcAttribute.Value;

                            // convert relative path to url
                            if (!src.StartsWith("http")) src = baseAddress + src;

                            //prevent duplicates
                            if (!pageContents.ImageUrls.Contains(src))
                                pageContents.ImageUrls.Add(src);
                        }

                        // Prepare for word count
                        // only concerned with <body> element
                        string textBody = document.DocumentNode.SelectSingleNode("//body").InnerHtml;

                        // remove HTML tags
                        textBody = Regex.Replace(textBody, "</?[a-z][a-z0-9]*[^<>]*>", " ");

                        // remove HTML comments
                        textBody = Regex.Replace(textBody, @"<!--(.|\s)*?-->", " ");

                        // remove script tags
                        textBody = Regex.Replace(textBody, "<script.*?</script>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        // remove styles
                        textBody = Regex.Replace(textBody, "<style.*?</style>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        // remove formatting
                        textBody = textBody.Replace("\n", "").Replace("\t", "").Replace("\r", "");

                        // grab all words
                        string[] words = textBody.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // total number of words
                        pageContents.WordCount = words.Length;

                        // compile counts by word
                        Dictionary<string, int> wordCounts = new Dictionary<string, int>();
                        foreach(string word in words)
                        {
                            if (wordCounts.ContainsKey(word))
                                wordCounts[word] += 1;
                            else
                                wordCounts.Add(word, 1);
                        }

                        // sort words by count
                        var wordCountsList = wordCounts.ToList();
                        wordCountsList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                        wordCountsList.Reverse();

                        // grab top 10 words
                        int top10 = (wordCountsList.Count > 10) ? 10 : wordCountsList.Count;
                        for (int i = 0; i<top10; i++)
                            pageContents.TopTen.Add(wordCountsList[i]);
                    }
                }
            }
            catch
            {
                return null;
            }

            return pageContents;
        }
    }
}
