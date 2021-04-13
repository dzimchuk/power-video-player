using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.ViewModel.MainView
{
    internal class DiscMenuViewModel
    {
        private readonly IDriveService _driveService;

        private readonly ICommand _cdRomCommand = new RelayCommand<DiscCommand>(
                d =>
                {
                    if (d != null)
                    {
                        Messenger.Default.Send(new PlayDiscMessage(d.DriveInfo.Name));
                    }
                },
                d => d != null && d.IsEnabled);

        private static readonly object _syncRoot = new object();

        public DiscMenuViewModel(IDriveService driveService)
        {
            _driveService = driveService;

            Messenger.Default.Register<EventMessage>(this, true, OnEventMessage);
        }

        private void OnEventMessage(EventMessage message)
        {
            if (message.Content == Event.ContextMenuOpened)
            {
                ScanDrives();
            }
        }

        private readonly ObservableCollection<DiscCommand> _discMenuItems = new ObservableCollection<DiscCommand>();
        public ObservableCollection<DiscCommand> DiscMenuItems
        {
            get { return _discMenuItems; }
        }

        private void PopulateDiscMenu(SynchronizationContext uiContext)
        {
            var drives = _driveService.GetAvailableCDRomDrives();

            uiContext.Send(state =>
                           {
                               lock (_syncRoot)
                               {
                                   _discMenuItems.Clear();

                                   foreach (var drive in drives)
                                   {
                                       _discMenuItems.Add(new DiscCommand(uiContext)
                                                          {
                                                              DriveInfo = drive,
                                                              Title = drive.Name,
                                                              Command = _cdRomCommand
                                                          });
                                   }
                               }
                           }, null);
        }

        private void ScanDrives()
        {
            ThreadPool.QueueUserWorkItem(state =>
                                         {
                                             PopulateDiscMenu((SynchronizationContext)state);

                                             lock (_syncRoot)
                                             {
                                                 var builder = new StringBuilder();

                                                 foreach (var item in _discMenuItems)
                                                 {
                                                     item.IsEnabled = false;
                                                     builder.Append(item.DriveInfo.Name);
                                                     try
                                                     {
                                                         if (item.DriveInfo.IsReady)
                                                         {
                                                             builder.Append(item.DriveInfo.VolumeLabel);

                                                             item.Title = builder.ToString();
                                                             item.IsEnabled = true;
                                                         }
                                                     }
                                                     catch
                                                     {
                                                     }

                                                     item.Title = builder.ToString();
                                                     builder.Remove(0, builder.Length);
                                                 }
                                             }

                                         }, SynchronizationContext.Current);
        }
    }
}