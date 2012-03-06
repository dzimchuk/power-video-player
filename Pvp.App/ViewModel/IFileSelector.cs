using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dzimchuk.Pvp.App.ViewModel
{
    public interface IFileSelector
    {
        string SelectFile(string filter);
    }
}
