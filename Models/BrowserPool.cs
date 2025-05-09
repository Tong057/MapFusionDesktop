using Microsoft.Playwright;

namespace MapFusion.Models
{
    public class BrowserPool : IDisposable
    {
        private readonly SemaphoreSlim semaphore;
        private readonly List<IBrowser> browsers;
        private readonly Random random = new Random();

        public BrowserPool(int poolSize)
        {
            semaphore = new SemaphoreSlim(poolSize, poolSize);
            browsers = new List<IBrowser>();
        }

        public async Task<IBrowser> RentBrowserAsync(IPlaywright playwright)
        {
            await semaphore.WaitAsync();

            lock (browsers)
            {
                var availableBrowser = browsers.FirstOrDefault(b => !IsBrowserInUse(b));
                if (availableBrowser != null)
                {
                    return availableBrowser;
                }
            }

            var launchOptions = new BrowserTypeLaunchOptions { Headless = true };

            if (AppSettings.UseProxy && AppSettings.ProxyList.Any())
            {
                var proxy = GetRandomProxy();
                if (proxy != null)
                {
                    launchOptions.Proxy = new Proxy
                    {
                        Server = $"http://{proxy.Host}:{proxy.Port}",
                    };
                }
            }

            var newBrowser = await playwright.Chromium.LaunchAsync(launchOptions);
            lock (browsers)
            {
                browsers.Add(newBrowser);
            }
            return newBrowser;
        }

        private bool IsBrowserInUse(IBrowser browser)
        {
            // Add your logic here to check if the browser is in use
            // Example: return !(browser as Browser)?.IsClosed ?? true;
            return false; // Placeholder, adjust as needed
        }

        public void ReturnBrowser(IBrowser browser)
        {
            lock (browsers)
            {
                // Reset browser state if needed
                // Example: browser.NewPageAsync();
            }
            semaphore.Release();
        }

        public void Dispose()
        {
            foreach (var browser in browsers)
            {
                (browser as IDisposable)?.Dispose();
            }
            semaphore.Dispose();
        }

        private ProxyModel GetRandomProxy()
        {
            lock (AppSettings.ProxyList)
            {
                if (AppSettings.ProxyList.Any())
                {
                    int index = random.Next(AppSettings.ProxyList.Count);
                    return AppSettings.ProxyList[index];
                }
                return null;
            }
        }
    }
}
