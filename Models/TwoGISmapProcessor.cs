using Microsoft.Playwright;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace MapFusion.Models
{
    public class TwoGISmapProcessor : BaseProcessor
    {
        public override string URL { get; }

        public TwoGISmapProcessor(string langCode, string query, int maxDepth) : base(langCode, query, maxDepth) { }

        public TwoGISmapProcessor(string url, int depth) : base(depth)
        {
            URL = url;
        }

        public async Task<List<TwoGISmapPlace>> ProcessAsync(IBrowser browser)
        {
            var places = new HashSet<TwoGISmapPlace>();
            var page = await browser.NewPageAsync();

            await page.GotoAsync(URL);
            await ClickRejectCookiesIfRequired(page);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            int currentPage = 1;
            bool hasNextPage = true;

            while (hasNextPage && currentPage <= MaxDepth)
            {
                // Парсинг ссылок на текущей странице
                var links = await GetLinksAsync(page);
                Debug.WriteLine($"{links.Count} links found on page {currentPage}");

                foreach (var link in links)
                {
                    places.Add(new TwoGISmapPlace($"https://2gis.ru{link}"));
                }

                Debug.WriteLine($"{places.Count} places found so far");

                // Переход на следующую страницу
                hasNextPage = await GoToNextPage(page, currentPage);
                currentPage++;
            }

            await page.CloseAsync();
            return places.ToList();
        }

        private async Task<List<string>> GetLinksAsync(IPage page)
        {
            // Ищем все ссылки на текущей странице, используя XPath или другой селектор, который подходит для сайта 2GIS
            var links = new List<string>();

            var elements = await page.QuerySelectorAllAsync("//a[contains(@href, '/firm/') or contains(@href, '/station/')]");
            foreach (var element in elements)
            {
                var href = await element.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href))
                {
                    links.Add(href);
                }
            }

            return links;
        }

        private async Task<bool> GoToNextPage(IPage page, int currentPage)
        {
            // Получаем все ссылки на страницы
            var pageLinks = await page.QuerySelectorAllAsync("a[href*='/search/']");
            Dictionary<int, IElementHandle> availablePages = new Dictionary<int, IElementHandle>();

            // Парсим номера страниц и сохраняем их в словаре
            foreach (var link in pageLinks)
            {
                var href = await link.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href))
                {
                    var match = Regex.Match(href, @".*/search/.*/page/(?<page_number>\d+)");
                    if (match.Success)
                    {
                        int pageNumber = int.Parse(match.Groups["page_number"].Value);
                        availablePages[pageNumber] = link;
                    }
                }
            }

            // Определяем номер следующей страницы
            int nextPageNumber = currentPage + 1;
            if (availablePages.ContainsKey(nextPageNumber))
            {
                var nextPageLink = availablePages[nextPageNumber];

                // Прокручиваем страницу до элемента, чтобы он стал видимым
                await nextPageLink.ScrollIntoViewIfNeededAsync();

                // Ждем некоторое время перед кликом
                await Task.Delay(500);

                // Кликаем по ссылке на следующую страницу
                await nextPageLink.ClickAsync();

                // Ждем загрузки новой страницы
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                return true;
            }
            else
            {
                Debug.WriteLine("No more pages found.");
                return false;
            }
        }

        protected override Task ScrollAsync(IPage page)
        {
            throw new NotImplementedException();
        }
    }
}
