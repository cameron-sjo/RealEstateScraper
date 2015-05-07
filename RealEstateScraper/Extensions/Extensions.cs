using System;

namespace RealEstateScraper.Extensions
{
  public static class Extensions
  {
    public static bool Between<T>(this T value, T lower, T upper) where T : IComparable<T>
    {
      return value.CompareTo(lower) >= 0 && value.CompareTo(upper) <= 0;
    }
  }
}