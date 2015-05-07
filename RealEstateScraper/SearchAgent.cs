using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RealEstateScraper.Contracts;
using RealEstateScraper.Extensions;
using RealEstateScraper.Models;

namespace RealEstateScraper
{
  internal class SearchAgent : ISearchAgent
  {
    public SearchAgent(IEnumerable<Listing> allListings)
    {
      Listings = allListings.ToList();
    }

    private List<Listing> Listings { get; set; }

    public IEnumerable<Listing> Search(string searchText)
    {
      try
      {
        var predicates = DismantleSearchText(searchText);

        return Listings.Where(listing => predicates.All(predicate => predicate.Invoke(listing)));
      }
      catch (Exception e)
      {
        throw new SearchException(e);
      }
    }

    private static IEnumerable<Predicate<Listing>> DismantleSearchText(string searchText)
    {
      const string regexPattern = @"((?:[a-z][a-z]+))(:)(("".*?"")|\w+(\.\.\.)?:?(\w+)|(\>|\<)?.\w+|(\w))";

      var results = new List<Predicate<Listing>>();

      foreach (Match match in Regex.Matches(searchText, regexPattern))
      {
        var split = match.Value.Split(':');

        var keyword = split[0].ToLower();

        Predicate<Listing> predicate;

        switch (keyword)
        {
          case "areainsquarefeet":
            predicate = GetRangePredicate(l => l.AreaInSquareFeet, match.Value);
            break;

          case"kind":
          case "category":
            predicate = GetValuePredicate(l => l.Category, match.Value);
            break;

          case "city":
            predicate = GetValuePredicate(l => l.City, match.Value);
            break;

          case "bathrooms":
          case "numberofbathrooms":
            predicate = GetRangePredicate(l => l.NumberOfBathrooms, match.Value);
            break;

          case "bedrooms":
          case "numberofbedrooms":
            predicate = GetRangePredicate(l => l.NumberOfBedrooms, match.Value);
            break;

          case "price":
            predicate = GetRangePredicate(l => l.Price, match.Value);
            break;

          case "squarefootage":
            predicate = GetRangePredicate(l => l.SquareFootage, match.Value);
            break;

          case "state":
            predicate = GetValuePredicate(l => l.State, match.Value);
            break;

          case "address":
          case "streetaddress":
            predicate = GetValuePredicate(l => l.StreetAddress, match.Value);
            break;

          case "webaddress":
            predicate = GetValuePredicate(l => l.WebAddress, match.Value);
            break;

          case "agent":
          case "agentname":
            predicate = GetValuePredicate(l => l.AgentName, match.Value);
            break;

          case "costperacre":
            predicate = GetRangePredicate(l => l.CostPerAcre, match.Value);
            break;

          case "keywords":
          case "description":
            predicate = GetValuePredicate(l => l.Description, match.Value);
            break;

          default:
            predicate = null;
            break;
        }

        if (predicate != null)
          results.Add(predicate);
      }

      return results;
    }

    private static string GetOperator(string value)
    {
      var condition = value.Split(':')[1];

      if (condition.Contains("..."))
        return "between";

      if (condition.Contains(">="))
        return "greaterthanorequalto";

      if (condition.Contains(">"))
        return "greaterthan";

      if (condition.Contains("<="))
        return "lessthanorequalto";

      if (condition.Contains("<"))
        return "lessthan";

      return "equals";
    }

    private static Predicate<Listing> GetRangePredicate(Func<Listing, decimal> func, string value)
    {
      var @operator = GetOperator(value);

      value = value.Split(':')[1];

      int index;
      decimal left;
      decimal right;

     switch (@operator)
      {
        case "between":
           index = value.IndexOf("...", StringComparison.Ordinal);
           left = Convert.ToDecimal(value.Substring(0, index));
           right = Convert.ToDecimal(value.Substring(index + 3));
          return l => func.Invoke(l).Between(left, right);

        case "greaterthanorequalto":
           index = value.IndexOf(">=", StringComparison.Ordinal);
           right = Convert.ToDecimal(value.Substring(index + 2));
          return l => func.Invoke(l) >= right;

        case "greaterthan":
          index = value.IndexOf(">", StringComparison.Ordinal);
          right = Convert.ToDecimal(value.Substring(index+1));
          return l => func.Invoke(l) > right;

        case "lessthanorequalto":
          index = value.IndexOf("<=", StringComparison.Ordinal);
          right = Convert.ToDecimal(value.Substring(index + 2));
          return l => func.Invoke(l) <= right;

        case "lessthan":
          index = value.IndexOf("<", StringComparison.Ordinal);
          right = Convert.ToDecimal(value.Substring(index + 1));
          return l => func.Invoke(l) < right;

        case "equals":
          index = value.IndexOf("=", StringComparison.Ordinal);
          right = Convert.ToDecimal(value.Substring(index + 1));
          return l => func.Invoke(l) == right;

        default:
          return l => false;
      }
    }

    private static Predicate<Listing> GetValuePredicate(Func<Listing, string> func, string value)
    {
      var condition = value.Split(':')[1];

      if (condition.Contains("\""))
      {
        condition = condition.Replace("\"", "");
      }

      condition = condition.ToLowerInvariant().Trim();

      return l => func.Invoke(l).ToLowerInvariant().Trim().Contains(condition);
    }
  }
}