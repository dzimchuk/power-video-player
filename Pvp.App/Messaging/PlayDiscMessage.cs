using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.Messaging
{
    internal class PlayDiscMessage : GenericMessage<string>
    {
        public PlayDiscMessage(string driveName) : base(driveName)
        {
        }
    }
}