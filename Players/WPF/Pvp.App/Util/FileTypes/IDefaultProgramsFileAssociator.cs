using System;

namespace Pvp.App.Util.FileTypes
{
    public interface IDefaultProgramsFileAssociator : IFileAssociator
    {
        bool LaunchAdvancedAssociationUI(string appName);
    }
}