using System.Collections.Generic;

namespace RealEstateScraper.Models
{
  public class Listing
  {
    public Listing()
    {
      OtherAmenities = new List<Amenity>();
    }

    public decimal AreaInSquareFeet { get; set; }

    public string Category { get; set; }

    public string City { get; set; }

    public string Description { get; set; }

    public string ImageAddress { get; set; }

    public decimal NumberOfBathrooms { get; set; }

    public decimal NumberOfBedrooms { get; set; }

    public List<Amenity> OtherAmenities { get; set; }

    public decimal Price { get; set; }

    public decimal SquareFootage { get; set; }

    public string State { get; set; }

    public string StreetAddress { get; set; }

    public string WebAddress { get; set; }

    public string AgentName { get; set; }

    public decimal CostPerAcre { get; set; }
  }
}