using System;

namespace Pvp.App.Util.FileTypes
{
    internal class DefaultProgramsFileAssociator8 : DefaultProgramsFileAssociator
    {
        public DefaultProgramsFileAssociator8(string docTypePrefix, string appName)
            : base(docTypePrefix, appName)
        {
        }

        public override bool CanAssociate
        {
            get
            {
                return false;
            }
        }

        public override bool IsAssociated(string ext)
        {
            throw new InvalidOperationException();
        }

        public override void Associate(string ext)
        {
            throw new InvalidOperationException();
        }

        public override void UnAssociate(string ext)
        {
            throw new InvalidOperationException();
        }
    }
}