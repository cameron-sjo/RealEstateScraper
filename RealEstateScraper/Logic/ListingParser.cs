using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using RealEstateScraper.Extensions;
using RealEstateScraper.Models;

namespace RealEstateScraper.Logic
{
  public static class ListingParser
  {
    public static IEnumerable<Listing> ParseDocument(HtmlDocument document)
    {
      var root = document.DocumentNode;

      var listingsRoot = root.Descendants("div")
                             .FirstOrDefault(n => n.GetAttributeValue("id") == "property-listing");

      if (listingsRoot == null)
      {
        Console.WriteLine("Unable to get listings");
        return null;
      }

      // This is a spot of volatility.
      // "item col-md-6" is very vague and generic.
      var descendants = listingsRoot.Descendants("div", "item col-md-6").ToList();

      return descendants.Select(ParseNode).ToList();
    }

    private static Listing ParseNode(HtmlNode node)
    {
      if (node == null)
      {
        return null;
      }

      var listing = new Listing();

      ParseImage(node, listing);
      ParsePriceAndType(node, listing);
      ParseInfo(node, listing);

      return listing;
    }

    private static void Map(string key, string value, Listing l)
    {
      key = key.ToLowerInvariant().Trim();

      switch (key)
      {
        case HtmlClassNames.Area:
          l.AreaInSquareFeet = ParseToDecimal(RemoveNonEssentialCharacters(value));
          break;

        case HtmlClassNames.Bedrooms:
          l.NumberOfBedrooms = ParseToDecimal(RemoveNonEssentialCharacters(value));
          break;

        case HtmlClassNames.Bathrooms:
          l.NumberOfBathrooms = ParseToDecimal(RemoveNonEssentialCharacters(value));
          break;

        case HtmlClassNames.SquareFootage:
          l.SquareFootage = ParseToDecimal(RemoveNonEssentialCharacters(value));
          break;

        default:
          if (l.OtherAmenities == null)
            l.OtherAmenities = new List<Amenity>();

          l.OtherAmenities.Add(new Amenity {Key = key, Value = value});
          break;
      }
    }

    private static void ParseAddress(HtmlNode node, Listing listing)
    {
      var root = node.Descendants("h3").FirstOrDefault();

      if (root == null)
      {
        Debug.WriteLine("Unable to find root info node.");
        return;
      }

      var addressNode = root.Descendants("a").FirstOrDefault();
      var locationNode = root.Descendants("small").FirstOrDefault();

      if (addressNode != null)
      {
        var webAddress = addressNode.GetAttributeValue("href");
        var streetAddress = addressNode.InnerText;

        listing.StreetAddress = streetAddress;
        listing.WebAddress = webAddress;
      }

      if (locationNode != null)
      {
        var location = locationNode.InnerText;

        var split = location.LastIndexOfAny(new[]
        {
          ' ',
          ','
        });

        if (split == -1)
          split = location.Length;

        listing.City = location.Substring(0, split).Trim();
        listing.State = location.Substring(split).Trim();
      }
    }

    private static void ParseAmenities(HtmlNode node, Listing listing)
    {
      var root = node.Descendants("ul", "amenities").FirstOrDefault();

      if (root == null)
      {
        Debug.WriteLine("Unable to find amenities.");
        return;
      }

      if (!root.HasChildNodes)
      {
        Debug.WriteLine("Listing appears to have no amenities...");
        return;
      }

      // For some odd reason, if I specified I wanted to use the 
      foreach (var amenityNode in root.Descendants("li"))
      {
        var key = amenityNode.Descendants("i").Select(n => n.GetAttributeValue("class")).FirstOrDefault();
        var value = amenityNode.InnerText;

        Map(key, value, listing);
      }
    }

    private static void ParseDescription(HtmlNode node, Listing listing)
    {
      var descNode = node.Descendants("p").FirstOrDefault();

      if (descNode == null)
      {
        Debug.WriteLine("Unable to find description node");
        return;
      }

      listing.Description = descNode.InnerText;
    }

    private static void ParseImage(HtmlNode listingNode, Listing listing)
    {
      var node = listingNode.Descendants("div", HtmlClassNames.Image).FirstOrDefault();

      if (node == null)
      {
        Debug.WriteLine("Unable to find image listingNode.");
        return;
      }

      var imageNode = node.Descendants("img")
                          .FirstOrDefault(n => n.HasAttributes &&
                                               n.GetAttributeValue("class") == "lazy" &&
                                               n.GetAttributeValue("data-original") != null);
      if (imageNode == null)
      {
        Debug.WriteLine("Unable to find image listingNode.");
        return;
      }

      var link = imageNode.GetAttributeValue("data-original", null);

      listing.ImageAddress = link;
    }

    private static void ParseInfo(HtmlNode node, Listing listing)
    {
      var infoNode = node.Descendants("div", "info").FirstOrDefault();

      if (infoNode == null)
      {
        Debug.WriteLine("Unable to find info node");
        return;
      }

      ParseAddress(infoNode, listing);
      ParseDescription(infoNode, listing);
      ParseAmenities(infoNode, listing);
    }

    private static void ParsePriceAndType(HtmlNode listingNode, Listing listing)
    {
      var node = listingNode.Descendants("div", HtmlClassNames.Price).FirstOrDefault();

      if (node == null)
        return;

      var type = node.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text &&
                                            !string.IsNullOrWhiteSpace(n.InnerText))
                     .Select(n => n.InnerText.Trim())
                     .FirstOrDefault();

      if (string.IsNullOrWhiteSpace(type))
        Debug.WriteLine("Unable to find type");

      listing.Category = type;

      var price = node.ChildNodes
                      .Where(n => n.Name == "span" && !string.IsNullOrWhiteSpace(n.InnerText))
                      .Select(n => n.InnerText)
                      .FirstOrDefault();

      if (string.IsNullOrWhiteSpace(price))
        return;

      listing.Price = decimal.Parse(price.Trim(), NumberStyles.AllowCurrencySymbol | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
    }

    private static decimal ParseToDecimal(string value)
    {
      return decimal.Parse(value, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
    }

    private static string RemoveNonEssentialCharacters(string value)
    {
      return new string(value.Where(c => Char.IsNumber(c) || Char.IsDigit(c) || c == '.' || c == ',').ToArray());
    }
  }
}