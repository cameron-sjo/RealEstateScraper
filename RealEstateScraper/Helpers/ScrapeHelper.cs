using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Polly;

namespace RealEstateScraper.Helpers
{
  public static class ScrapeHelper
  {
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

    private static async Task<string> GetStringAsync(string uri)
    {
      var policy = Policy
        .Handle<WebException>()
        .WaitAndRetryAsync(new[]
        {
          TimeSpan.FromSeconds(3),
          TimeSpan.FromSeconds(5),
          TimeSpan.FromSeconds(8)
        }, (exception, span) =>
        {
          Console.WriteLine("Failed to connect to website.");
          Console.WriteLine("Error message: {0}", exception.Message);
          Console.WriteLine("Waiting for {0} seconds", span.TotalSeconds);
        });

      return await policy.ExecuteAsync(async () =>
      {
        using (var client = new HttpClient())
        {
          var val = await client.GetStringAsync(uri);

          return val;
        }
      });
    }
  }
}