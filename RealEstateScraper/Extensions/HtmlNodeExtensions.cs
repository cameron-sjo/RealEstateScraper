using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace RealEstateScraper.Extensions
{
  public static class HtmlNodeExtensions
  {
    /// <remarks>
    ///   Rather than replicating a query that I'm going to be using a lot, add it here, and provide the specific class value
    ///   that I'm looking for.
    /// </remarks>
    public static IEnumerable<HtmlNode> Descendants(this HtmlNode node, string name, string @class)
    {
      return node.Descendants(name)
                 .Where(n => n.HasAttributes &&
                             n.GetAttributeValue("class") == @class);
    }

    /// <remarks>
    ///   Rather than having to specify the default value for what I'm trying to equate to every time, I will default to null.
    ///   Thus, create an extension to "clean" the implementations up, and improve readability.
    /// </remarks>
    public static string GetAttributeValue(this HtmlNode node, string attributeName)
    {
      return node.GetAttributeValue(attributeName, null);
    }
  }
}