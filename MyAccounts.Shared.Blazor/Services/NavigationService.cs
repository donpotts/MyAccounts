using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Shared.Blazor.Services
{
    public class NavigationService
    {
        public event Action OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
