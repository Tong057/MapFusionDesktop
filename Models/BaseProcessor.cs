using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MapFusion.Models
{
    public abstract class BaseProcessor
    {
        public string LangCode { get; private set; }
        public string Query { get; private set; }
        public int MaxDepth { get; private set; }
        public abstract string URL { get; }

        public BaseProcessor(string langCode, string query, int maxDepth)
        {
            LangCode = langCode;
            Query = query;
            MaxDepth = maxDepth;
        }

        public BaseProcessor(int depth)
        {
            MaxDepth = depth;
        }

        //public abstract Task<List<BasePlace>> ProcessAsync(IBrowser browser);

        protected abstract Task ScrollAsync(IPage page);

        public static async Task ClickRejectCookiesIfRequired(IPage page)
        {
            try
            {
                var cookieButton = await page.QuerySelectorAsync("form[action='https://consent.google.com/save']:first-of-type button:first-of-type");
                if (cookieButton != null)
                {
                    await cookieButton.ClickAsync();
                }
            }
            catch (Exception)
            {
                // Ignore errors if the cookie button is not found
            }
        }
    }
}
