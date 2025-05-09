using Microsoft.Playwright;
using System.Diagnostics;
using System.Net;

namespace MapFusion.Models
{
    public class GmapProcessor : BaseProcessor
    {
        public override string URL => $"https://www.google.com/maps/search/{WebUtility.UrlEncode(Query)}?hl={LangCode}";

        public GmapProcessor(string langCode, string query, int maxDepth) : base(langCode, query, maxDepth) { }

        public async Task<List<GmapPlace>> ProcessAsync(IBrowser browser)
        {
            var places = new HashSet<GmapPlace>();
            var page = await browser.NewPageAsync();

            await page.GotoAsync(URL);
            await ClickRejectCookiesIfRequired(page);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            await ScrollAsync(page);

            var links = await page.QuerySelectorAllAsync("div[role=feed] div[jsaction]>a");
            foreach (var link in links)
            {
                var href = await link.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href) && href.Contains("/maps/place/"))
                {
                    places.Add(new GmapPlace(href));
                }
            }

            Debug.WriteLine($"{places.Count} places found");

            await page.CloseAsync();

            return places.ToList();
        }

        protected override async Task ScrollAsync(IPage page)
        {
            string scrollSelector = "div[role='feed']";
            string script = @$"async () => {{
            const el = document.querySelector(""{scrollSelector}"");
            el.scrollTop = el.scrollHeight;

            return new Promise((resolve) => {{
                setTimeout(() => {{
                    resolve(el.scrollHeight);
                }}, 500);
            }});
        }}";

            int currentScrollHeight = 0;
            double waitTime = 100;
            int cnt = 0;

            const int timeout = 1000;
            const int maxWait2 = 2000;

            for (int i = 0; i < MaxDepth; i++)
            {
                cnt++;
                double waitTime2 = timeout * cnt;

                if (waitTime2 > maxWait2)
                {
                    waitTime2 = maxWait2;
                }

                var scrollHeight = await page.EvaluateAsync<int>(script);

                //if (scrollHeight == currentScrollHeight)
                //{
                //    break;
                //}

                currentScrollHeight = scrollHeight;

                waitTime *= 1.5;

                if (waitTime > maxWait2)
                {
                    waitTime = maxWait2;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(waitTime));
            }
        }

    }
}
