using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapFusion.Models
{
    public abstract class BasePlace
    {
        public string URL { get; set; }

        public string Title { get; set; }
        public List<string> Categories { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string PhoneNumber { get; set; }
        public string PlusCode { get; set; }
        public int ReviewCount { get; set; }
        public double ReviewRating { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Cid { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ReviewsLink { get; set; }
        public string ThumbnailImage { get; set; }
        public string Timezone { get; set; }
        public string PriceRange { get; set; }
        public LinkSource Menu { get; set; }
        public Owner Owner { get; set; }
        public Address CompleteAddress { get; set; }
        public List<Image> Images { get; set; }
        public List<About> Abouts { get; set; }
        public List<Review> UserReviews { get; set; } = new List<Review>();
        public List<LinkSource> Reservations { get; set; }
        public Dictionary<int, int> ReviewsPerRating { get; set; }
        public Dictionary<string, List<string>> OpenHours { get; set; }
        public Dictionary<string, Dictionary<int, int>> PopularTimes { get; set; }


        public BasePlace(string url)
        {
            URL = url;
        }

        public abstract Task ProcessAsync(IBrowser browser);
        public abstract Task<JsonElement?> ScrollAsync(IPage page);
    }

    public class GetLinkSourceParams
    {
        public JArray Arr { get; set; }
        public List<int> Source { get; set; }
        public List<int> Link { get; set; }
    }

    public class Review
    {
        public string Name { get; set; }
        public string ProfilePicture { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
        public List<string> Images { get; set; }
        public string When { get; set; }
    }

    public class Option
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public class About
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<Option> Options { get; set; }
    }

    public class Image 
    {
        public string Title { get; set; }
        public string ImageLink { get; set; }
    }

    public class LinkSource
    {
        public string Link { get; set; }
        public string Source { get; set; }
    }

    public class Owner
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
    }

    public class Address
    {
        public string Borough { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}
