using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RealEstateScraper.Contracts;
using RealEstateScraper.Helpers;
using RealEstateScraper.Logic;
using RealEstateScraper.Models;

namespace RealEstateScraper
{
  public class ListingAgent : IListingAgent
  {
    private const string HomePage = @"http://marketedgerealty.com/";
    private const string GenericAgent = HomePage + @"agent/";

    public async Task<IEnumerable<Listing>> GetAllListingsAsync()
    {
      var agents = await GetAgents();

      var tasks = agents.Select(a => GetAgentListingsAsync(a.Name, a.ListingsWebAddress)).ToArray();

      await Task.WhenAll(tasks);

      return tasks.SelectMany(s => s.Result);
    }

    public async Task<IEnumerable<Listing>> GetListingsForAgentAsync(string agentName)
    {
      agentName = NormalizeAgentName(agentName);

      if (string.IsNullOrWhiteSpace(agentName) ||
          agentName == "all")
      {
        return await GetAllListingsAsync();
      }

      return await GetAgentListingsAsync(agentName, string.Format("{0}{1}", GenericAgent, agentName));
    }

    private static async Task<List<Listing>> GetAgentListingsAsync(string agentName, string agentWebAddress)
    {
      if (string.IsNullOrWhiteSpace(agentWebAddress))
      {
        return null;
      }

      Debug.WriteLine("Attempting generation of listings for {0}", agentWebAddress);

      var listings = new List<Listing>(await ScrapeHelper.ScrapeAsync(agentWebAddress, ListingParser.ParseDocument));

      foreach (var listing in listings)
      {
        listing.AgentName = agentName;
      }

      Debug.WriteLine("Generated {0} listings from {1}", listings.Count(), agentWebAddress);

      return listings;
    }

    private static async Task<IEnumerable<Agent>> GetAgents()
    {
      var foundAgents = await ScrapeHelper.ScrapeAsync(HomePage, AgentParser.Parse);

      Debug.WriteLine("Found {0} agents.", foundAgents.Count());

      return foundAgents;
    }

    private static string NormalizeAgentName(string original)
    {
      return original.ToLowerInvariant()
                     .Trim()
                     .Replace(" ", "-");
    }
  }
}