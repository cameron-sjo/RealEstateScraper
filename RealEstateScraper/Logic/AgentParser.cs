using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using RealEstateScraper.Models;

namespace RealEstateScraper.Logic
{
  public static class AgentParser
  {
    public static List<Agent> Parse(HtmlNode root)
    {
      var agents = new List<Agent>();

      agents.AddRange(GetAgentsFromNavbar(root));

      return agents;
    }

    private static IEnumerable<Agent> GetAgentsFromNavbar(HtmlNode root)
    {
      var agents = new List<Agent>();
      
      var agentNodes = root.Descendants("li")
                           .Where(n => n.GetAttributeValue("class", string.Empty).Contains(HtmlClassNames.AgentMenuItem)).ToList();

      foreach (var agentNode in agentNodes)
      {
        var linkNode = agentNode.Descendants("a")
                                .SelectMany(n => n.Attributes.AttributesWithName("href"))
                                .FirstOrDefault();

        var agentName = agentNode.InnerText;

        string listingsLink = null;

        if (linkNode == null)
          Debug.WriteLine("Unable to get link for {0}.", agentName);
        else
          listingsLink = linkNode.Value;

        var agent = new Agent
        {
          ListingsWebAddress = listingsLink,
          Name = agentName
        };

        agents.Add(agent);
      }

      return agents;
    }
  }
}