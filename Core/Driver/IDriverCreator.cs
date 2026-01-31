using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Driver
{
    public interface IDriverCreator
    {
        string BrowserName { get; }
        IWebDriver Create(bool headless = false);
    }
}
