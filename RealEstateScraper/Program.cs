using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Nito.AsyncEx;
using RealEstateScraper.Contracts;
using RealEstateScraper.Models;

namespace RealEstateScraper
{
  // ReSharper disable once ClassNeverInstantiated.Global
  internal class Program
  {
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

    private static async Task GetListings(string name)
    {
      try
      {
        Task<IEnumerable<Listing>> task;

        IListingAgent agent = new ListingAgent();

        if (string.IsNullOrWhiteSpace(name) ||
            name.ToLowerInvariant().Trim() == "all")
        {
          task = agent.GetAllListingsAsync();

          name = "all agents";
        }
        else
        {
          task = agent.GetListingsForAgentAsync(name);

          name = name.Titleize();
        }

        var listings = await task;

        Console.WriteLine("{0} {1} were found for {2}",
                          listings.Count(),
                          "listing".Pluralize(),
                          name);
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error has occurred.");
        Console.WriteLine("Error message: {0}", ex.Message);
      }
    }
  }
}