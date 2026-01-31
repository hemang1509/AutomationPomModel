using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Driver
{
    public interface IDriverFactory
    {
        IWebDriver CreateDriver(string browserType, bool headless = false);
    }
}
