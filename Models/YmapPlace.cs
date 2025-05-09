using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;

namespace MapFusion.Models
{
    public class YmapPlace : BasePlace
    {

        public YmapPlace(string url) : base(url) { }

        public override async Task ProcessAsync(IBrowser browser)
        {
            
        }

        public override Task<JsonElement?> ScrollAsync(IPage page)
        {
            throw new NotImplementedException();
        }
    }
}
