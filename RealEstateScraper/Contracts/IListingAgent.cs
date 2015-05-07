using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateScraper.Models;

namespace RealEstateScraper.Contracts
{
  public interface IListingAgent
  {
    Task<IEnumerable<Listing>> GetAllListingsAsync();

    Task<IEnumerable<Listing>> GetListingsForAgentAsync(string agentName);
  }

  public interface ISearchAgent
  {
    IEnumerable<Listing> Search(string searchText);
  }
}