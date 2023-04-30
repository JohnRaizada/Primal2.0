using System.Collections.Generic;

namespace PrimalEditor.Utilities
{
    enum Platforms
    {
        Android,
        ChromeOS,
        iOS,
        Mac,
        Windows,
        Linux
    }
    enum Headings
    {
        Name,
        ChromeOS,
        ChromeO,
    }
    class SDKManager
    {
        public List<ExpanderListMenuItem> MyItems { get; set; }

        public SDKManager()
        {
            MyItems = new List<ExpanderListMenuItem>
        {
            new ExpanderListMenuItem { Name = "Item1", APILevel = 1 },
            new ExpanderListMenuItem { Name = "Item2", APILevel = 2 },
            // Add more items here
        };

        }

    }
}
