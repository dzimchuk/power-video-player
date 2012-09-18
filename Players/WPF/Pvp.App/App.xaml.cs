using System;
using System.Linq;
using System.Windows;
using Pvp.App.Composition;
using Pvp.App.Util;
using Pvp.App.View;
using Ninject;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Guid _appGuid = new Guid("{631D2885-2074-4804-9BBB-BAC7CEB94993}");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var si = new SingleInstance(_appGuid);
            si.ArgsRecieved += si_ArgsRecieved;
            si.Run(() =>
            {
                SetUpDependencies();
                new MainWindow().Show();
                return MainWindow;
            }, e.Args);
        }

        private void si_ArgsRecieved(string[] args)
        {
            if (args.Length > 0)
                MainWindow.Content += string.Format("Recieved args: {0}\n", args.Aggregate((all, arg) => all += (" " + arg)));
        }

        private void SetUpDependencies()
        {
            var kernel = new StandardKernel();
            kernel.Load(typeof(App).Assembly);

            var resolver = new NinjectDependencyResolver(kernel);
            DependencyResolver.SetResolver(resolver);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            Messenger.Default.Send(new EventMessage(Event.SessionEnding));
        }
    }
}
