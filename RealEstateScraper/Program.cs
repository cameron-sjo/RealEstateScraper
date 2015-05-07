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

        var listings = (await GetListings(name)).ToList();

        if (listings.Any())
          SearchListings(listings);
      });
    }

    private static async Task<IEnumerable<Listing>> GetListings(string name)
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

        Console.WriteLine();
        Console.WriteLine("{0} {1} were found for {2}",
                          listings.Count(),
                          "listing".Pluralize(),
                          name);

        return listings;
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error has occurred.");
        Console.WriteLine("Error message: {0}", ex.Message);

        return Enumerable.Empty<Listing>();
      }
    }

    private static void SearchListings(IEnumerable<Listing> listings)
    {
      var searchAgent = new SearchAgent(listings.ToList());

      do
      {
        Console.WriteLine();
        Console.WriteLine("Enter search query or 'exit': ");
        var searchText = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(searchText) ||
            searchText.Trim().ToLowerInvariant() == "exit")
        {
          break;
        }

        try
        {
          var foundListings = searchAgent.Search(searchText).ToArray();

          Console.WriteLine();

          if (!foundListings.Any())
          {
            Console.WriteLine("No listings found matching query.");
          }
          else
          {
            Console.WriteLine("Listings found matching query: ");

            for (var index = 1; index <= foundListings.Length; index++)
            {
              var listing = foundListings[index - 1];

              var address = string.Format("{0} {1} {2}",
                                          listing.StreetAddress,
                                          listing.City,
                                          listing.State)
                                  .ToLowerInvariant()
                                  .Titleize();

              Console.WriteLine(" {3,3}. Address: {0, -30} Price: {1,-7} Kind: {2,-12}",
                                address.PadRight(30, ' ').Substring(0, 30),
                                listing.Price,
                                (listing.Category ?? "").Titleize(),
                                index);
            }
          }
        }
        catch (SearchException e)
        {
          Console.WriteLine("There was a problem with your search query. Please try again.");
        }
      } while (true);
    }
  }
}