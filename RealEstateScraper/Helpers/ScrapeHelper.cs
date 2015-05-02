using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace RealEstateScraper.Helpers
{
  public static class ScrapeHelper
  {
    private static async Task<string> GetStringAsync(string uri)
    {
      using (var client = new HttpClient())
      {
        var val = await client.GetStringAsync(uri);

        return val;
      }
    }

    public static async Task<T> ScrapeAsync<T>(string address, Func<HtmlNode, T> action)
    {
      return action.Invoke(await ScrapeAsync(address, doc => doc.DocumentNode));
    }

    public static async Task<T> ScrapeAsync<T>(string address, Func<HtmlDocument, T> action)
    {
      var html = await GetStringAsync(address);

      var document = new HtmlDocument();

      document.LoadHtml(html);

      var result = action.Invoke(document);

      return result;
    }
  }
}