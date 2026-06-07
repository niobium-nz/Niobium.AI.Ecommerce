using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Niobium.AI.Web;

namespace Niobium.AI.Playwright
{
    internal sealed class PlaywrightBrowserDriver(IOptions<PlaywrightBrowserLaunchOptions> options, IPlaywright playwright) : IWebBrowser
    {
        private readonly List<IPage> pages = [];
        private readonly Dictionary<IPage, List<WebBrowserConsoleMessage>> consoleMessages = [];
        private readonly Dictionary<IPage, List<WebBrowserNetworkRequest>> networkRequests = [];
        private readonly SemaphoreSlim syncLock = new(1, 1);
        private IBrowser? browser;
        private IBrowserContext? browserContext;
        private bool disposed;

        public async Task<WebBrowserTabInfo> OpenTabAsync(CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            await this.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            IPage page = await this.browserContext!.NewPageAsync().ConfigureAwait(false);

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return this.RegisterPage(page);
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        public async Task CloseTabAsync(int tabId, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            await page.CloseAsync().ConfigureAwait(false);

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.UnregisterPage(page);
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        public async Task<IReadOnlyList<WebBrowserTabInfo>> GetTabsAsync(CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<IPage> pages = await this.GetPagesSnapshotAsync(cancellationToken).ConfigureAwait(false);

            List<WebBrowserTabInfo> tabs = [];
            for (int i = 0; i < pages.Count; i++)
            {
                tabs.Add(await CreateTabInfoAsync(pages[i], i).ConfigureAwait(false));
            }

            return tabs.AsReadOnly();
        }

        public async Task<WebBrowserNavigationResult> NavigateAsync(int tabId, string url, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            if (String.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(url));
            }

            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            await page.GotoAsync(url).ConfigureAwait(false);
            return await this.CreateNavigationResultAsync(page, tabId).ConfigureAwait(false);
        }

        public async Task<WebBrowserPageSnapshot> SnapshotAsync(int tabId, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            string content = await page.ContentAsync().ConfigureAwait(false);
            string title = await page.TitleAsync().ConfigureAwait(false);
            JsonElement document = await page.EvaluateAsync<JsonElement>("""
                () => ({
                    documentElement: document.documentElement?.outerHTML ?? null,
                    bodyText: document.body?.innerText ?? '',
                    forms: Array.from(document.forms).map((form, index) => ({
                        index,
                        id: form.id || null,
                        name: form.getAttribute('name'),
                        method: form.getAttribute('method'),
                        action: form.getAttribute('action'),
                        fields: Array.from(form.elements).map((element, fieldIndex) => ({
                            index: fieldIndex,
                            tagName: element.tagName,
                            type: element.getAttribute('type'),
                            name: element.getAttribute('name'),
                            id: element.id || null,
                            value: 'value' in element ? element.value : null
                        }))
                    })),
                    links: Array.from(document.links).map((link, index) => ({
                        index,
                        text: link.textContent?.trim() ?? '',
                        href: link.href,
                        title: link.title || null
                    }))
                })
                """).ConfigureAwait(false);
            return new WebBrowserPageSnapshot
            {
                Tab = await CreateTabInfoAsync(page, tabId).ConfigureAwait(false),
                Title = title,
                Url = page.Url,
                Html = content,
                Document = JsonSerializer.Deserialize<WebBrowserDocumentSnapshot>(document, SerializationOptions.SnakeCase)
                    ?? throw new InvalidOperationException("Unable to deserialize the page snapshot."),
            };
        }

        public async Task WaitForAsync(int tabId, WebBrowserWaitForRequest waitCondition, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(waitCondition);
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            switch (waitCondition.Target)
            {
                case WebBrowserWaitTarget.Selector:
                    if (String.IsNullOrWhiteSpace(waitCondition.Value))
                    {
                        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions
                        {
                            Timeout = waitCondition.TimeoutMs,
                        }).ConfigureAwait(false);
                        break;
                    }
                    else
                    {
                        await page.WaitForSelectorAsync(waitCondition.Value, new PageWaitForSelectorOptions
                        {
                            State = MapSelectorState(waitCondition.State),
                            Timeout = waitCondition.TimeoutMs,
                        }).ConfigureAwait(false);
                    }
                    break;
                case WebBrowserWaitTarget.LoadState:
                    await page.WaitForLoadStateAsync(MapLoadState(waitCondition.LoadState ?? Niobium.AI.Web.WebBrowserLoadState.Load), new PageWaitForLoadStateOptions
                    {
                        Timeout = waitCondition.TimeoutMs,
                    }).ConfigureAwait(false);
                    break;
                case WebBrowserWaitTarget.Timeout:
                    await page.WaitForTimeoutAsync(waitCondition.TimeoutMs).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported wait target: {waitCondition.Target}.");
            }
        }

        public async Task ClickAsync(int tabId, string selector, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            if (String.IsNullOrWhiteSpace(selector))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(selector));
            }

            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            await page.ClickAsync(selector).ConfigureAwait(false);
        }

        public async Task FillFormAsync(int tabId, IReadOnlyList<WebBrowserFormFieldInput> formFields, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(formFields);
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            foreach (WebBrowserFormFieldInput field in formFields)
            {
                ArgumentNullException.ThrowIfNull(field);
                if (String.IsNullOrWhiteSpace(field.Selector))
                {
                    throw new ArgumentException("Field selector cannot be null or whitespace.", nameof(formFields));
                }

                await page.FillAsync(field.Selector, field.Value ?? String.Empty).ConfigureAwait(false);
            }
        }

        public async Task TypeAsync(int tabId, string selector, string text, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            if (String.IsNullOrWhiteSpace(selector))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(selector));
            }

            ArgumentNullException.ThrowIfNull(text);
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            await page.FillAsync(selector, text).ConfigureAwait(false);
        }

        public async Task SelectOptionAsync(int tabId, string selector, IReadOnlyList<string> optionValues, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            if (String.IsNullOrWhiteSpace(selector))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(selector));
            }

            ArgumentNullException.ThrowIfNull(optionValues);
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            await page.SelectOptionAsync(selector, optionValues.Select(v => new SelectOptionValue { Value = v }).ToArray()).ConfigureAwait(false);
        }

        public async Task<WebBrowserScreenshotResult> TakeScreenshotAsync(int tabId, WebBrowserScreenshotRequest screenshotRequest, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(screenshotRequest);
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            string outputPath = screenshotRequest.Path ?? String.Empty;
            if (String.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine(Path.GetTempPath(), $"playwright-{Guid.NewGuid():N}.png");
            }

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = screenshotRequest.FullPage,
                Path = outputPath,
                Timeout = screenshotRequest.TimeoutMs,
                Type = ScreenshotType.Png,
            }).ConfigureAwait(false);

            return new WebBrowserScreenshotResult
            {
                Tab = await CreateTabInfoAsync(page, tabId).ConfigureAwait(false),
                Path = outputPath,
                Uri = new Uri(outputPath),
            };
        }

        public Task SetDialogHandlerAsync(int tabId, WebBrowserDialogHandlingRequest dialogHandling, CancellationToken cancellationToken = default)
            => this.SetDialogHandlerCoreAsync(tabId, dialogHandling, cancellationToken);

        private async Task SetDialogHandlerCoreAsync(int tabId, WebBrowserDialogHandlingRequest dialogHandling, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(dialogHandling);
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);
            page.Dialog += async (_, dialog) =>
            {
                if (dialogHandling.Accept)
                {
                    await dialog.AcceptAsync(dialogHandling.PromptText).ConfigureAwait(false);
                    return;
                }

                await dialog.DismissAsync().ConfigureAwait(false);
            };
        }

        public async Task<IReadOnlyList<WebBrowserConsoleMessage>> GetConsoleMessagesAsync(int tabId, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IReadOnlyList<WebBrowserConsoleMessage> messages = this.consoleMessages.TryGetValue(page, out List<WebBrowserConsoleMessage>? value)
                    ? new ReadOnlyCollection<WebBrowserConsoleMessage>(value.ToList())
                    : Array.Empty<WebBrowserConsoleMessage>();
                return messages;
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        public async Task<IReadOnlyList<WebBrowserNetworkRequest>> GetNetworkRequestsAsync(int tabId, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IReadOnlyList<WebBrowserNetworkRequest> requests = this.networkRequests.TryGetValue(page, out List<WebBrowserNetworkRequest>? value)
                    ? new ReadOnlyCollection<WebBrowserNetworkRequest>(value.ToList())
                    : Array.Empty<WebBrowserNetworkRequest>();
                return requests;
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        public async Task<WebBrowserEvaluationResult> EvaluateAsync(int tabId, string javaScriptExpression, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            IPage page = await this.GetPageAsync(tabId, cancellationToken).ConfigureAwait(false);

            if (String.IsNullOrWhiteSpace(javaScriptExpression))
            {
                return new WebBrowserEvaluationResult
                {
                    IsError = true,
                    Tab = await CreateTabInfoAsync(page, tabId).ConfigureAwait(false),
                    JsonResult = JsonSerializer.Serialize(new { error = "javascript expression cannot be null or whitespace." }),
                };
            }

            try
            {
                JsonElement result = await page.EvaluateAsync<JsonElement>(javaScriptExpression).ConfigureAwait(false);
                return new WebBrowserEvaluationResult
                {
                    IsError = false,
                    Tab = await CreateTabInfoAsync(page, tabId).ConfigureAwait(false),
                    JsonResult = JsonSerializer.Serialize(result, SerializationOptions.SnakeCase),
                };
            }
            catch (PlaywrightException e)
            {
                return new WebBrowserEvaluationResult
                {
                    IsError = true,
                    Tab = await CreateTabInfoAsync(page, tabId).ConfigureAwait(false),
                    JsonResult = JsonSerializer.Serialize(new { error = e.Message }),
                };
            }
        }

        public Task<WebBrowserEvaluationResult> RunCodeAsync(int tabId, string javaScriptCode, CancellationToken cancellationToken = default)
            => this.EvaluateAsync(tabId, javaScriptCode, cancellationToken);

        public void Dispose()
            => this.DisposeAsync().AsTask().GetAwaiter().GetResult();

        public async ValueTask DisposeAsync()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            if (this.browserContext is not null)
            {
                await this.browserContext.CloseAsync().ConfigureAwait(false);
            }

            if (this.browser is not null)
            {
                await this.browser.CloseAsync().ConfigureAwait(false);
            }

            this.syncLock.Dispose();
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (this.browserContext is not null)
            {
                return;
            }

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this.browserContext is not null)
                {
                    return;
                }

                IBrowserType browserType = GetBrowserType(playwright, options.Value.Browser);
                string? browserChannel = GetBrowserChannel(options.Value.Browser);
                ViewportSize viewportSize = new() { Width = options.Value.ViewportWidth, Height = options.Value.ViewportHeight };

                if (String.IsNullOrWhiteSpace(options.Value.UserDataDir))
                {
                    this.browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = options.Value.Headless,
                        Channel = browserChannel,
                        ExecutablePath = options.Value.ExecutablePath,
                        SlowMo = options.Value.SlowMo,
                        Timeout = options.Value.TimeoutMs,
                    }).ConfigureAwait(false);

                    this.browserContext = await this.browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        AcceptDownloads = options.Value.AcceptDownloads,
                        BaseURL = options.Value.BaseUrl,
                        Locale = options.Value.Locale,
                        UserAgent = options.Value.UserAgent,
                        ViewportSize = viewportSize,
                    }).ConfigureAwait(false);
                }
                else
                {
                    this.browserContext = await browserType.LaunchPersistentContextAsync(options.Value.UserDataDir, new BrowserTypeLaunchPersistentContextOptions
                    {
                        Headless = options.Value.Headless,
                        Channel = browserChannel,
                        ExecutablePath = options.Value.ExecutablePath,
                        SlowMo = options.Value.SlowMo,
                        Timeout = options.Value.TimeoutMs,

                        AcceptDownloads = options.Value.AcceptDownloads,
                        BaseURL = options.Value.BaseUrl,
                        Locale = options.Value.Locale,
                        UserAgent = options.Value.UserAgent,
                        ViewportSize = viewportSize,
                    }).ConfigureAwait(false);
                }

                IPage page = await this.browserContext.NewPageAsync().ConfigureAwait(false);
                this.RegisterPage(page);
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        private static IBrowserType GetBrowserType(IPlaywright playwright, WebBrowserKind browser)
            => browser switch
            {
                WebBrowserKind.Edge => playwright.Chromium,
                WebBrowserKind.Chrome => playwright.Chromium,
                WebBrowserKind.Firefox => playwright.Firefox,
                WebBrowserKind.Webkit => playwright.Webkit,
                _ => throw new InvalidOperationException($"Unsupported browser kind: {browser}.")
            };

        private static string? GetBrowserChannel(WebBrowserKind browser)
            => browser switch
            {
                WebBrowserKind.Edge => "msedge",
                WebBrowserKind.Chrome => "chrome",
                _ => null
            };

        private static WaitForSelectorState? MapSelectorState(Niobium.AI.Web.WebBrowserWaitForSelectorState? state)
            => state switch
            {
                Niobium.AI.Web.WebBrowserWaitForSelectorState.Attached => WaitForSelectorState.Attached,
                Niobium.AI.Web.WebBrowserWaitForSelectorState.Detached => WaitForSelectorState.Detached,
                Niobium.AI.Web.WebBrowserWaitForSelectorState.Visible => WaitForSelectorState.Visible,
                Niobium.AI.Web.WebBrowserWaitForSelectorState.Hidden => WaitForSelectorState.Hidden,
                _ => null
            };

        private static LoadState MapLoadState(Niobium.AI.Web.WebBrowserLoadState state)
            => state switch
            {
                Niobium.AI.Web.WebBrowserLoadState.Load => LoadState.Load,
                Niobium.AI.Web.WebBrowserLoadState.DOMContentLoaded => LoadState.DOMContentLoaded,
                Niobium.AI.Web.WebBrowserLoadState.NetworkIdle => LoadState.NetworkIdle,
                _ => LoadState.Load
            };

        private WebBrowserTabInfo RegisterPage(IPage page)
        {
            this.pages.Add(page);
            this.consoleMessages[page] = [];
            this.networkRequests[page] = [];

            page.Console += (_, message) =>
            {
                if (!this.consoleMessages.TryGetValue(page, out List<WebBrowserConsoleMessage>? messages))
                {
                    return;
                }

                messages.Add(new WebBrowserConsoleMessage
                {
                    Text = message.Text,
                    Type = message.Type,
                });
            };

            page.Request += (_, request) =>
            {
                if (!this.networkRequests.TryGetValue(page, out List<WebBrowserNetworkRequest>? requests))
                {
                    return;
                }

                requests.Add(new WebBrowserNetworkRequest
                {
                    Method = request.Method,
                    Headers = new ReadOnlyDictionary<string, string>(request.Headers),
                    IsNavigationRequest = request.IsNavigationRequest,
                    RequestBody = request.PostData,
                    Url = request.Url,
                    ResourceType = request.ResourceType,
                });
            };

            page.Response += async (_, response) =>
            {
                WebBrowserNetworkRequest? requestInfo = this.networkRequests.TryGetValue(page, out List<WebBrowserNetworkRequest>? requests)
                    ? requests.LastOrDefault(r => String.Equals(r.Url, response.Url, StringComparison.Ordinal))
                    : null;

                if (requestInfo is null)
                {
                    return;
                }

                requestInfo.Response = new WebBrowserNetworkResponse
                {
                    Headers = new ReadOnlyDictionary<string, string>(await response.AllHeadersAsync().ConfigureAwait(false)),
                    Ok = response.Ok,
                    Status = response.Status,
                    StatusText = response.StatusText,
                    Url = response.Url,
                };
            };

            page.Close += async (_, _) => await this.UnregisterPageAsync(page).ConfigureAwait(false);
            return new WebBrowserTabInfo
            {
                Id = this.pages.Count - 1,
                Url = page.Url,
                Title = String.Empty,
            };
        }

        private void UnregisterPage(IPage page)
        {
            _ = this.pages.Remove(page);
            _ = this.consoleMessages.Remove(page);
            _ = this.networkRequests.Remove(page);
        }

        private async Task<IReadOnlyList<IPage>> GetPagesSnapshotAsync(CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return this.pages.ToList().AsReadOnly();
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        private async Task<IPage> GetPageAsync(int tabId, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            await this.syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return this.GetPage(tabId);
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        private IPage GetPage(int tabId)
        {
            this.ThrowIfDisposed();
            return tabId < 0 || tabId >= this.pages.Count ? throw new ArgumentOutOfRangeException(nameof(tabId)) : this.pages[tabId];
        }

        private static async Task<WebBrowserTabInfo> CreateTabInfoAsync(IPage page, int tabId)
            => new()
            {
                Id = tabId,
                Url = page.Url,
                Title = await page.TitleAsync().ConfigureAwait(false),
            };

        private async Task<WebBrowserNavigationResult> CreateNavigationResultAsync(IPage page, int tabId)
            => new()
            {
                Tab = await CreateTabInfoAsync(page, tabId).ConfigureAwait(false),
                Url = page.Url,
            };

        private async Task UnregisterPageAsync(IPage page)
        {
            if (this.disposed)
            {
                return;
            }

            await this.syncLock.WaitAsync().ConfigureAwait(false);
            try
            {
                this.UnregisterPage(page);
            }
            finally
            {
                this.syncLock.Release();
            }
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(this.disposed, this);
    }
}
