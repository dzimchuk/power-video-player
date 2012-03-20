using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.App.ViewModel
{
    public interface IFileSelector
    {
        string SelectFile(string filter);
    }
}
