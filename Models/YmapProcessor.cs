using Microsoft.Playwright;
using System.Diagnostics;
using System.Net;

namespace MapFusion.Models
{
    public class YmapProcessor : BaseProcessor
    {
        private string url;
        private int selectedDepth;

        public override string URL => $"https://yandex.ru/maps/?mode=search&text={WebUtility.UrlEncode(Query)}";

        public YmapProcessor(string langCode, string query, int maxDepth) : base(langCode, query, maxDepth) { }


        public async Task<List<YmapPlace>> ProcessAsync(IBrowser browser)
        {
            var places = new HashSet<YmapPlace>();
            var page = await browser.NewPageAsync();

            await page.GotoAsync(URL);
            await ClickRejectCookiesIfRequired(page);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            await ScrollAsync(page);

            var links = await page.QuerySelectorAllAsync("span[data-nosnippet] a.search-snippet-view__link-overlay");
            Debug.WriteLine($"{links.Count()} links found");
            foreach (var link in links)
            {
                var href = await link.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href) && href.Contains("/maps/org/"))
                {
                    places.Add(new YmapPlace($"https://yandex.ru{href}"));
                }
            }

            Debug.WriteLine($"{places.Count} places found");

            await page.CloseAsync();

            return places.ToList();
        }

        protected override async Task ScrollAsync(IPage page)
        {
            string scrollSelector = ".scroll__container";
            string script = @$"async () => {{
        const el = document.querySelector(""{scrollSelector}"");
        const distance = el.scrollHeight - el.scrollTop;
        el.scrollBy(0, distance);

        return new Promise((resolve) => {{
            setTimeout(() => {{
                resolve(el.scrollHeight);
            }}, 500);
        }});
    }}";

            int currentScrollHeight = 0;
            double waitTime = 1000;
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

                if (scrollHeight == currentScrollHeight)
                {
                    break;
                }

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
