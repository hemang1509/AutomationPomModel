using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Pages
{
    /// <summary>
    /// Base page class providing common functionality for all page objects
    /// Implements: DRY (Don't Repeat Yourself), Single Responsibility
    /// </summary>
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected readonly Actions Actions;
        protected readonly IJavaScriptExecutor JsExecutor;

        // Default timeout values
        protected const int DefaultTimeout = 10;
        protected const int ShortTimeout = 5;
        protected const int LongTimeout = 30;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of BasePage
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="timeoutSeconds">Default timeout for waits</param>
        protected BasePage(IWebDriver driver, int timeoutSeconds = DefaultTimeout)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };
            Actions = new Actions(driver);
            JsExecutor = (IJavaScriptExecutor)driver;

            // Ignore specific exceptions during waits
            Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        }

        #endregion

        #region Element Finding Methods

        /// <summary>
        /// Finds a single element
        /// </summary>
        protected IWebElement FindElement(By locator)
        {
            try
            {
                return Driver.FindElement(locator);
            }
            catch (NoSuchElementException ex)
            {
                throw new NoSuchElementException($"Element not found: {locator}", ex);
            }
        }

        /// <summary>
        /// Finds multiple elements
        /// </summary>
        protected ReadOnlyCollection<IWebElement> FindElements(By locator)
        {
            return Driver.FindElements(locator);
        }

        /// <summary>
        /// Checks if element exists (without throwing exception)
        /// </summary>
        protected bool IsElementPresent(By locator)
        {
            try
            {
                Driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if element is displayed (without throwing exception)
        /// </summary>
        protected bool IsElementDisplayed(By locator)
        {
            try
            {
                return Driver.FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if element is enabled (without throwing exception)
        /// </summary>
        protected bool IsElementEnabled(By locator)
        {
            try
            {
                return Driver.FindElement(locator).Enabled;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        #endregion

        #region Wait Methods

        /// <summary>
        /// Waits for element to be visible
        /// </summary>
        protected IWebElement WaitForElementToBeVisible(By locator, int timeoutSeconds = DefaultTimeout)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

                return wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(locator);
                        return element.Displayed ? element : null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new WebDriverTimeoutException($"Element not visible within {timeoutSeconds} seconds: {locator}", ex);
            }
        }

        /// <summary>
        /// Waits for element to be clickable
        /// </summary>
        protected IWebElement WaitForElementToBeClickable(By locator, int timeoutSeconds = DefaultTimeout)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

                return wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(locator);
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new WebDriverTimeoutException($"Element not clickable within {timeoutSeconds} seconds: {locator}", ex);
            }
        }

        /// <summary>
        /// Waits for element to be invisible
        /// </summary>
        protected bool WaitForElementToBeInvisible(By locator, int timeoutSeconds = DefaultTimeout)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

                return wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(locator);
                        return !element.Displayed;
                    }
                    catch (NoSuchElementException)
                    {
                        return true;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return true;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for element to exist in DOM
        /// </summary>
        protected IWebElement WaitForElementToExist(By locator, int timeoutSeconds = DefaultTimeout)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

                return wait.Until(driver =>
                {
                    try
                    {
                        return driver.FindElement(locator);
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new WebDriverTimeoutException($"Element not found in DOM within {timeoutSeconds} seconds: {locator}", ex);
            }
        }

        /// <summary>
        /// Waits for text to be present in element
        /// </summary>
        protected bool WaitForTextToBePresentInElement(By locator, string text, int timeoutSeconds = DefaultTimeout)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

                return wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(locator);
                        return element.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
                    }
                    catch (NoSuchElementException)
                    {
                        return false;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for page to load completely
        /// </summary>
        protected void WaitForPageToLoad(int timeoutSeconds = LongTimeout)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver =>
                JsExecutor.ExecuteScript("return document.readyState").Equals("complete"));
        }

        /// <summary>
        /// Waits for jQuery/AJAX to complete
        /// </summary>
        protected void WaitForAjaxToComplete(int timeoutSeconds = DefaultTimeout)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver =>
                    (bool)JsExecutor.ExecuteScript("return (typeof jQuery != 'undefined') ? jQuery.active == 0 : true"));
            }
            catch (Exception)
            {
                // jQuery might not be present, continue
            }
        }

        /// <summary>
        /// Custom wait with custom condition
        /// </summary>
        protected T WaitUntil<T>(Func<IWebDriver, T> condition, int timeoutSeconds = DefaultTimeout)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(condition);
        }

        #endregion

        #region Click Methods

        /// <summary>
        /// Clicks on an element (with wait)
        /// </summary>
        protected void Click(By locator)
        {
            try
            {
                WaitForElementToBeClickable(locator).Click();
            }
            catch (ElementClickInterceptedException)
            {
                // Retry with JavaScript click
                ClickWithJavaScript(locator);
            }
            catch (StaleElementReferenceException)
            {
                // Retry once
                Thread.Sleep(500);
                WaitForElementToBeClickable(locator).Click();
            }
        }

        /// <summary>
        /// Clicks using JavaScript
        /// </summary>
        protected void ClickWithJavaScript(By locator)
        {
            var element = WaitForElementToExist(locator);
            JsExecutor.ExecuteScript("arguments[0].click();", element);
        }

        /// <summary>
        /// Double clicks on an element
        /// </summary>
        protected void DoubleClick(By locator)
        {
            var element = WaitForElementToBeClickable(locator);
            Actions.DoubleClick(element).Perform();
        }

        /// <summary>
        /// Right clicks on an element
        /// </summary>
        protected void RightClick(By locator)
        {
            var element = WaitForElementToBeClickable(locator);
            Actions.ContextClick(element).Perform();
        }

        /// <summary>
        /// Clicks and holds an element
        /// </summary>
        protected void ClickAndHold(By locator)
        {
            var element = WaitForElementToBeClickable(locator);
            Actions.ClickAndHold(element).Perform();
        }

        /// <summary>
        /// Releases a held click
        /// </summary>
        protected void Release(By locator)
        {
            var element = WaitForElementToBeVisible(locator);
            Actions.Release(element).Perform();
        }

        #endregion

        #region Input Methods

        /// <summary>
        /// Types text into an input field (with clear)
        /// </summary>
        protected void Type(By locator, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var element = WaitForElementToBeVisible(locator);
            element.Clear();
            element.SendKeys(text);
        }

        /// <summary>
        /// Types text slowly (character by character)
        /// </summary>
        protected void TypeSlowly(By locator, string text, int delayMilliseconds = 100)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var element = WaitForElementToBeVisible(locator);
            element.Clear();

            foreach (char c in text)
            {
                element.SendKeys(c.ToString());
                Thread.Sleep(delayMilliseconds);
            }
        }

        /// <summary>
        /// Appends text without clearing
        /// </summary>
        protected void AppendText(By locator, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var element = WaitForElementToBeVisible(locator);
            element.SendKeys(text);
        }

        /// <summary>
        /// Clears an input field
        /// </summary>
        protected void Clear(By locator)
        {
            var element = WaitForElementToBeVisible(locator);
            element.Clear();
        }

        /// <summary>
        /// Types text using JavaScript (for difficult inputs)
        /// </summary>
        protected void TypeWithJavaScript(By locator, string text)
        {
            var element = WaitForElementToExist(locator);
            JsExecutor.ExecuteScript($"arguments[0].value = '{text.Replace("'", "\\'")}';", element);

            // Trigger input event for frameworks like React/Angular
            JsExecutor.ExecuteScript("arguments[0].dispatchEvent(new Event('input', { bubbles: true }));", element);
        }

        /// <summary>
        /// Sends keyboard keys (like Enter, Tab, etc.)
        /// </summary>
        protected void SendKeys(By locator, string keys)
        {
            var element = WaitForElementToBeVisible(locator);
            element.SendKeys(keys);
        }

        /// <summary>
        /// Presses Enter key on element
        /// </summary>
        protected void PressEnter(By locator)
        {
            SendKeys(locator, Keys.Enter);
        }

        /// <summary>
        /// Presses Tab key on element
        /// </summary>
        protected void PressTab(By locator)
        {
            SendKeys(locator, Keys.Tab);
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// Gets text from an element
        /// </summary>
        protected string GetText(By locator)
        {
            return WaitForElementToBeVisible(locator).Text;
        }

        /// <summary>
        /// Gets attribute value from an element
        /// </summary>
        protected string GetAttribute(By locator, string attributeName)
        {
            return WaitForElementToBeVisible(locator).GetAttribute(attributeName);
        }

        /// <summary>
        /// Gets value from input field
        /// </summary>
        protected string GetValue(By locator)
        {
            return GetAttribute(locator, "value") ?? string.Empty;
        }

        /// <summary>
        /// Gets CSS property value
        /// </summary>
        protected string GetCssValue(By locator, string propertyName)
        {
            return WaitForElementToBeVisible(locator).GetCssValue(propertyName);
        }

        /// <summary>
        /// Gets all text from multiple elements
        /// </summary>
        protected List<string> GetAllTexts(By locator)
        {
            var elements = FindElements(locator);
            return elements.Select(e => e.Text).ToList();
        }

        /// <summary>
        /// Gets element count
        /// </summary>
        protected int GetElementCount(By locator)
        {
            return FindElements(locator).Count;
        }

        #endregion

        #region Dropdown/Select Methods

        /// <summary>
        /// Selects option by visible text
        /// </summary>
        protected void SelectByText(By locator, string text)
        {
            var element = WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            select.SelectByText(text);
        }

        /// <summary>
        /// Selects option by value
        /// </summary>
        protected void SelectByValue(By locator, string value)
        {
            var element = WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            select.SelectByValue(value);
        }

        /// <summary>
        /// Selects option by index
        /// </summary>
        protected void SelectByIndex(By locator, int index)
        {
            var element = WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            select.SelectByIndex(index);
        }

        /// <summary>
        /// Gets selected option text
        /// </summary>
        protected string GetSelectedOptionText(By locator)
        {
            var element = WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            return select.SelectedOption.Text;
        }

        /// <summary>
        /// Gets selected option value
        /// </summary>
        protected string GetSelectedOptionValue(By locator)
        {
            var element = WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            return select.SelectedOption.GetAttribute("value");
        }

        /// <summary>
        /// Gets all dropdown options
        /// </summary>
        protected List<string> GetAllDropdownOptions(By locator)
        {
            var element = WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            return select.Options.Select(o => o.Text).ToList();
        }

        /// <summary>
        /// Checks if dropdown contains option
        /// </summary>
        protected bool DropdownContainsOption(By locator, string optionText)
        {
            var options = GetAllDropdownOptions(locator);
            return options.Contains(optionText);
        }

        #endregion

        #region Checkbox/Radio Methods

        /// <summary>
        /// Checks a checkbox (if not already checked)
        /// </summary>
        protected void Check(By locator)
        {
            var element = WaitForElementToBeClickable(locator);
            if (!element.Selected)
            {
                element.Click();
            }
        }

        /// <summary>
        /// Unchecks a checkbox (if currently checked)
        /// </summary>
        protected void Uncheck(By locator)
        {
            var element = WaitForElementToBeClickable(locator);
            if (element.Selected)
            {
                element.Click();
            }
        }

        /// <summary>
        /// Checks if checkbox/radio is selected
        /// </summary>
        protected bool IsSelected(By locator)
        {
            return WaitForElementToBeVisible(locator).Selected;
        }

        /// <summary>
        /// Toggles checkbox state
        /// </summary>
        protected void Toggle(By locator)
        {
            WaitForElementToBeClickable(locator).Click();
        }

        #endregion

        #region Mouse Actions

        /// <summary>
        /// Hovers over an element
        /// </summary>
        protected void Hover(By locator)
        {
            var element = WaitForElementToBeVisible(locator);
            Actions.MoveToElement(element).Perform();
        }

        /// <summary>
        /// Drags and drops from source to target
        /// </summary>
        protected void DragAndDrop(By sourceLocator, By targetLocator)
        {
            var source = WaitForElementToBeVisible(sourceLocator);
            var target = WaitForElementToBeVisible(targetLocator);
            Actions.DragAndDrop(source, target).Perform();
        }

        /// <summary>
        /// Scrolls to element
        /// </summary>
        protected void ScrollToElement(By locator)
        {
            var element = WaitForElementToExist(locator);
            JsExecutor.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", element);
            Thread.Sleep(500); // Allow time for scroll animation
        }

        /// <summary>
        /// Scrolls to element and clicks
        /// </summary>
        protected void ScrollAndClick(By locator)
        {
            ScrollToElement(locator);
            Click(locator);
        }

        /// <summary>
        /// Scrolls to top of page
        /// </summary>
        protected void ScrollToTop()
        {
            JsExecutor.ExecuteScript("window.scrollTo({top: 0, behavior: 'smooth'});");
            Thread.Sleep(500);
        }

        /// <summary>
        /// Scrolls to bottom of page
        /// </summary>
        protected void ScrollToBottom()
        {
            JsExecutor.ExecuteScript("window.scrollTo({top: document.body.scrollHeight, behavior: 'smooth'});");
            Thread.Sleep(500);
        }

        /// <summary>
        /// Scrolls by pixel offset
        /// </summary>
        protected void ScrollBy(int xPixels, int yPixels)
        {
            JsExecutor.ExecuteScript($"window.scrollBy({xPixels}, {yPixels});");
            Thread.Sleep(300);
        }

        #endregion

        #region Alert Methods

        /// <summary>
        /// Waits for alert to be present
        /// </summary>
        protected IAlert WaitForAlert(int timeoutSeconds = DefaultTimeout)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(driver => driver.SwitchTo().Alert());
        }

        /// <summary>
        /// Accepts alert
        /// </summary>
        protected void AcceptAlert(int timeoutSeconds = DefaultTimeout)
        {
            WaitForAlert(timeoutSeconds).Accept();
        }

        /// <summary>
        /// Dismisses alert
        /// </summary>
        protected void DismissAlert(int timeoutSeconds = DefaultTimeout)
        {
            WaitForAlert(timeoutSeconds).Dismiss();
        }

        /// <summary>
        /// Gets alert text
        /// </summary>
        protected string GetAlertText(int timeoutSeconds = DefaultTimeout)
        {
            return WaitForAlert(timeoutSeconds).Text;
        }

        /// <summary>
        /// Types text into alert prompt
        /// </summary>
        protected void TypeInAlert(string text, int timeoutSeconds = DefaultTimeout)
        {
            var alert = WaitForAlert(timeoutSeconds);
            alert.SendKeys(text);
            alert.Accept();
        }

        /// <summary>
        /// Checks if alert is present
        /// </summary>
        protected bool IsAlertPresent()
        {
            try
            {
                Driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }

        #endregion

        #region Frame Methods

        /// <summary>
        /// Switches to frame by index
        /// </summary>
        protected void SwitchToFrame(int index)
        {
            Driver.SwitchTo().Frame(index);
        }

        /// <summary>
        /// Switches to frame by name or ID
        /// </summary>
        protected void SwitchToFrame(string nameOrId)
        {
            Driver.SwitchTo().Frame(nameOrId);
        }

        /// <summary>
        /// Switches to frame by element
        /// </summary>
        protected void SwitchToFrame(By locator)
        {
            var frameElement = WaitForElementToBeVisible(locator);
            Driver.SwitchTo().Frame(frameElement);
        }

        /// <summary>
        /// Switches back to default content
        /// </summary>
        protected void SwitchToDefaultContent()
        {
            Driver.SwitchTo().DefaultContent();
        }

        /// <summary>
        /// Switches to parent frame
        /// </summary>
        protected void SwitchToParentFrame()
        {
            Driver.SwitchTo().ParentFrame();
        }

        #endregion

        #region Window Methods

        /// <summary>
        /// Gets current window handle
        /// </summary>
        protected string GetCurrentWindowHandle()
        {
            return Driver.CurrentWindowHandle;
        }

        /// <summary>
        /// Gets all window handles
        /// </summary>
        protected ReadOnlyCollection<string> GetAllWindowHandles()
        {
            return Driver.WindowHandles;
        }

        /// <summary>
        /// Switches to window by title
        /// </summary>
        protected void SwitchToWindowByTitle(string title)
        {
            var windows = Driver.WindowHandles;
            foreach (var window in windows)
            {
                Driver.SwitchTo().Window(window);
                if (Driver.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            throw new NoSuchWindowException($"Window with title '{title}' not found");
        }

        /// <summary>
        /// Switches to window by partial title
        /// </summary>
        protected void SwitchToWindowByPartialTitle(string partialTitle)
        {
            var windows = Driver.WindowHandles;
            foreach (var window in windows)
            {
                Driver.SwitchTo().Window(window);
                if (Driver.Title.Contains(partialTitle, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            throw new NoSuchWindowException($"Window with partial title '{partialTitle}' not found");
        }

        /// <summary>
        /// Switches to latest window
        /// </summary>
        protected void SwitchToLatestWindow()
        {
            var windows = Driver.WindowHandles;
            Driver.SwitchTo().Window(windows.Last());
        }

        /// <summary>
        /// Switches to window by index
        /// </summary>
        protected void SwitchToWindow(int index)
        {
            var windows = Driver.WindowHandles;
            if (index < windows.Count)
            {
                Driver.SwitchTo().Window(windows[index]);
            }
            else
            {
                throw new NoSuchWindowException($"Window at index {index} not found. Total windows: {windows.Count}");
            }
        }

        /// <summary>
        /// Closes current window and switches back to main window
        /// </summary>
        protected void CloseCurrentWindow()
        {
            Driver.Close();
            var windows = Driver.WindowHandles;
            if (windows.Count > 0)
            {
                Driver.SwitchTo().Window(windows[0]);
            }
        }

        /// <summary>
        /// Closes all windows except main window
        /// </summary>
        protected void CloseAllWindowsExceptMain()
        {
            var mainWindow = Driver.WindowHandles[0];
            var allWindows = Driver.WindowHandles;

            foreach (var window in allWindows)
            {
                if (window != mainWindow)
                {
                    Driver.SwitchTo().Window(window);
                    Driver.Close();
                }
            }

            Driver.SwitchTo().Window(mainWindow);
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigates to URL
        /// </summary>
        public void NavigateTo(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            Driver.Navigate().GoToUrl(url);
            WaitForPageToLoad();
        }

        /// <summary>
        /// Refreshes the page
        /// </summary>
        public void Refresh()
        {
            Driver.Navigate().Refresh();
            WaitForPageToLoad();
        }

        /// <summary>
        /// Navigates back
        /// </summary>
        public void GoBack()
        {
            Driver.Navigate().Back();
            WaitForPageToLoad();
        }

        /// <summary>
        /// Navigates forward
        /// </summary>
        public void GoForward()
        {
            Driver.Navigate().Forward();
            WaitForPageToLoad();
        }

        #endregion

        #region Page Information Methods

        /// <summary>
        /// Gets current URL
        /// </summary>
        public string GetCurrentUrl()
        {
            return Driver.Url;
        }

        /// <summary>
        /// Gets page title
        /// </summary>
        public string GetPageTitle()
        {
            return Driver.Title;
        }

        /// <summary>
        /// Gets page source
        /// </summary>
        public string GetPageSource()
        {
            return Driver.PageSource;
        }

        /// <summary>
        /// Checks if URL contains text
        /// </summary>
        public bool UrlContains(string text)
        {
            return GetCurrentUrl().Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if title contains text
        /// </summary>
        public bool TitleContains(string text)
        {
            return GetPageTitle().Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region JavaScript Methods

        /// <summary>
        /// Executes JavaScript
        /// </summary>
        protected object ExecuteScript(string script, params object[] args)
        {
            return JsExecutor.ExecuteScript(script, args);
        }

        /// <summary>
        /// Executes async JavaScript
        /// </summary>
        protected object ExecuteAsyncScript(string script, params object[] args)
        {
            return JsExecutor.ExecuteAsyncScript(script, args);
        }

        /// <summary>
        /// Highlights element (for debugging)
        /// </summary>
        protected void HighlightElement(By locator)
        {
            var element = FindElement(locator);
            var originalStyle = element.GetAttribute("style") ?? string.Empty;

            JsExecutor.ExecuteScript(
                "arguments[0].setAttribute('style', 'border: 3px solid red; background-color: yellow;');",
                element);

            Thread.Sleep(500);

            JsExecutor.ExecuteScript(
                $"arguments[0].setAttribute('style', '{originalStyle}');",
                element);
        }

        /// <summary>
        /// Removes element attribute (useful for removing readonly, disabled, etc.)
        /// </summary>
        protected void RemoveAttribute(By locator, string attributeName)
        {
            var element = WaitForElementToExist(locator);
            JsExecutor.ExecuteScript($"arguments[0].removeAttribute('{attributeName}');", element);
        }

        /// <summary>
        /// Sets element attribute
        /// </summary>
        protected void SetAttribute(By locator, string attributeName, string value)
        {
            var element = WaitForElementToExist(locator);
            JsExecutor.ExecuteScript($"arguments[0].setAttribute('{attributeName}', '{value}');", element);
        }

        #endregion

        #region Screenshot Methods

        /// <summary>
        /// Takes screenshot of current page
        /// </summary>
        public byte[] TakeScreenshot()
        {
            return ((ITakesScreenshot)Driver).GetScreenshot().AsByteArray;
        }

        /// <summary>
        /// Takes screenshot and saves to file
        /// </summary>
        public string TakeScreenshot(string filePath)
        {
            try
            {
                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save screenshot to {filePath}", ex);
            }
        }

        /// <summary>
        /// Takes screenshot with timestamp
        /// </summary>
        public string TakeScreenshotWithTimestamp(string directory, string prefix = "screenshot")
        {
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var fileName = $"{prefix}_{timestamp}.png";
            var filePath = System.IO.Path.Combine(directory, fileName);

            return TakeScreenshot(filePath);
        }

        #endregion

        #region Cookie Methods

        /// <summary>
        /// Gets all cookies
        /// </summary>
        protected ReadOnlyCollection<Cookie> GetAllCookies()
        {
            return Driver.Manage().Cookies.AllCookies;
        }

        /// <summary>
        /// Gets cookie by name
        /// </summary>
        protected Cookie GetCookie(string name)
        {
            return Driver.Manage().Cookies.GetCookieNamed(name);
        }

        /// <summary>
        /// Adds a cookie
        /// </summary>
        protected void AddCookie(Cookie cookie)
        {
            Driver.Manage().Cookies.AddCookie(cookie);
        }

        /// <summary>
        /// Deletes cookie by name
        /// </summary>
        protected void DeleteCookie(string name)
        {
            Driver.Manage().Cookies.DeleteCookieNamed(name);
        }

        /// <summary>
        /// Deletes all cookies
        /// </summary>
        protected void DeleteAllCookies()
        {
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Waits for specified seconds
        /// </summary>
        protected void WaitInSeconds(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }

        /// <summary>
        /// Waits for specified milliseconds
        /// </summary>
        protected void WaitInMilliseconds(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Generates random string
        /// </summary>
        protected string GenerateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates random email
        /// </summary>
        protected string GenerateRandomEmail(string domain = "example.com")
        {
            return $"test_{GenerateRandomString(8)}@{domain}";
        }

        /// <summary>
        /// Generates random number
        /// </summary>
        protected int GenerateRandomNumber(int min = 1, int max = 1000)
        {
            var random = new Random();
            return random.Next(min, max);
        }

        /// <summary>
        /// Gets current timestamp
        /// </summary>
        protected string GetTimestamp(string format = "yyyy-MM-dd_HH-mm-ss")
        {
            return DateTime.Now.ToString(format);
        }

        #endregion
    }
}
