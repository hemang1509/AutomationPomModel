using Core.Driver;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Drivers
{
    /// <summary>
    /// Factory for creating browser drivers using Strategy Pattern
    /// Implements: Open/Closed, Dependency Inversion, Single Responsibility
    /// OPEN for extension (add new browser) but CLOSED for modification
    /// </summary>
    public class DriverFactory : IDriverFactory
    {
        private readonly Dictionary<string, IDriverCreator> _driverCreators;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="driverCreators">Collection of driver creators to register</param>
        public DriverFactory(IEnumerable<IDriverCreator> driverCreators)
        {
            if (driverCreators == null || !driverCreators.Any())
            {
                throw new ArgumentException("At least one driver creator must be provided", nameof(driverCreators));
            }

            _driverCreators = driverCreators.ToDictionary(
                creator => creator.BrowserName.ToLower(),
                creator => creator,
                StringComparer.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// Parameterless constructor for backward compatibility
        /// Automatically registers Chrome, Firefox, and Edge
        /// </summary>
        public DriverFactory() : this(GetDefaultDriverCreators())
        {
        }

        /// <summary>
        /// Creates a WebDriver instance for the specified browser
        /// </summary>
        /// <param name="browserType">Browser name (chrome, firefox, edge)</param>
        /// <param name="headless">Run in headless mode</param>
        /// <returns>Configured WebDriver instance</returns>
        public IWebDriver CreateDriver(string browserType, bool headless = false)
        {
            if (string.IsNullOrWhiteSpace(browserType))
            {
                throw new ArgumentException("Browser type cannot be null or empty", nameof(browserType));
            }

            var normalizedBrowserType = browserType.ToLower().Trim();

            if (!_driverCreators.TryGetValue(normalizedBrowserType, out var creator))
            {
                var supportedBrowsers = string.Join(", ", _driverCreators.Keys);
                throw new ArgumentException(
                    $"Browser type '{browserType}' is not supported. Supported browsers: {supportedBrowsers}",
                    nameof(browserType)
                );
            }

            try
            {
                return creator.Create(headless);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create {browserType} driver. " +
                    $"Ensure the WebDriver executable is installed and compatible with your browser version. " +
                    $"Error: {ex.Message}",
                    ex
                );
            }
        }

        /// <summary>
        /// Gets the list of supported browser names
        /// </summary>
        public IEnumerable<string> GetSupportedBrowsers()
        {
            return _driverCreators.Keys;
        }

        /// <summary>
        /// Returns default driver creators (Chrome, Firefox, Edge)
        /// </summary>
        private static IEnumerable<IDriverCreator> GetDefaultDriverCreators()
        {
            return new List<IDriverCreator>
            {
                new ChromeDriverCreator()
               
            };
        }
    }
}