using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Shared.Blazor.Services
{
    public class ThemeService
    {
        public bool IsDarkMode { get; set; }

        public void SetDarkMode(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;
        }
    }
}