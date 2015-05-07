using System;

namespace RealEstateScraper
{
  public class SearchException : Exception
  {
    public SearchException(Exception innerException) : base("There was a problem with your search query. Please try again.", innerException) {}

    public SearchException() {}
  }
}