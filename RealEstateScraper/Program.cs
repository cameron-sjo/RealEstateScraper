using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RealEstateScraper.Contracts;
using RealEstateScraper.Models;

namespace RealEstateScraper
{
  internal class Program
  {
    private static async Task GetListings(string name)
    {
      Task<IEnumerable<Listing>> task;

      IListingAgent agent = new ListingAgent();

      if (string.IsNullOrWhiteSpace(name) ||
          name.ToLowerInvariant().Trim() == "all")
      {
        task = agent.GetAllListingsAsync();
      }
      else
      {
        task = agent.GetListingsForAgentAsync(name);
      }

      var listings = await task;

      Console.WriteLine("{0} listings were found for {1}", listings.Count(), name);
    }

    public static void Main(string[] args)
    {
      AsyncContext.Run(async () =>
      {
        Console.WriteLine("Please enter the name of agent for whose listings you wish to get: ");

        var name = Console.ReadLine();

        await GetListings(name);

        Console.ReadLine();
      });
    }
  }
}