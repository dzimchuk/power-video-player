using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using Pvp.App.Composition;
using Pvp.App.Messaging;
using Pvp.App.Util;
using Pvp.App.View;
using Pvp.App.ViewModel;

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
            SetUpDependencies();

            if (Array.Find(e.Args,
                           delegate(string arg)
                               {
                                   string s = arg.ToLowerInvariant();
                                   return s == "-regapp" || s == "/regapp";
                               }
                    ) != default(string))
            {
                HandleRegApp();
                Shutdown();
            }
            else if (Array.Find(e.Args,
                                delegate(string arg)
                                    {
                                        string s = arg.ToLowerInvariant();
                                        return s == "-unregapp" || s == "/unregapp";
                                    }
                         ) != default(string))
            {
                HandleUnRegApp();
                Shutdown();
            }

            var si = new SingleInstance(_appGuid);
            si.ArgsRecieved += si_ArgsRecieved;
            si.Run(() =>
                       {
                           new MainWindow().Show();
                           return MainWindow;
                       }, e.Args);

            SynchronizationContext.Current.Post(state => { si_ArgsRecieved((string[])state); }, e.Args);
        }

        private void si_ArgsRecieved(string[] args)
        {
            if (args.Any())
                Messenger.Default.Send(new PlayNewFileMessage(args[0]));
        }

        private static void SetUpDependencies()
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

        public static void HandleRegApp()
        {
            if (IsAdmin)
            {
                CultureInfo ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                string strExe = Assembly.GetExecutingAssembly().Location;
                string command = "\"" + strExe + "\"";
                string icon = strExe + ",0";

                string appName = Pvp.App.Resources.Resources.program_name;
                string appDescription = Pvp.App.Resources.Resources.program_description;

                var fileAssociator = DependencyResolver.Current.Resolve<IFileAssociatorRegistration>();
                fileAssociator.Register(@"Software\Clients\Media", icon, command, appName, appDescription, GetTypesInfo());
            }
            else
            {
                Elevate("-regapp");
            }
        }

        public static void HandleUnRegApp()
        {
            if (IsAdmin)
            {
                var fileAssociator = DependencyResolver.Current.Resolve<IFileAssociatorRegistration>();
                fileAssociator.Unregister();
            }
            else
            {
                Elevate("-unregapp");
            }
        }

        private static void Elevate(string command)
        {
            // do not elevate if it's already being elevated, i.e. if -elevate is specified we won't try to run another process (it shouldn't happen but an extra protection won't harm)
            if (Array.Find(Environment.GetCommandLineArgs(), delegate(string arg)
                                                                 {
                                                                     string s = arg.ToLowerInvariant();
                                                                     return s == "-elevate" || s == "/elevate";
                                                                 }
                    ) == default(string)) // no -elevate option is found
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Assembly.GetExecutingAssembly().Location;
                startInfo.Verb = "runas";
                startInfo.Arguments = "-elevate " + command;
                try
                {
                    Process p = Process.Start(startInfo);
                    p.WaitForExit();
                }
                catch
                    // System.ComponentModel.Win32Exception is thrown if the user refused to run the application with admin permissions; but we also guard against other errors
                {
                    // just exit if there is any problem or the user denied the elevation
                }
            }
        }

        private static IEnumerable<FileAssociator.DocTypeInfo> GetTypesInfo()
        {
            string strExe = Assembly.GetExecutingAssembly().Location;
            string command = "\"" + strExe + "\" \"%L\"";
            string icon = strExe + ",0";

            return (from t in FileTypes.All
                    let description = Pvp.App.Resources.Resources.ResourceManager.GetString(string.Format("file_type_{0}", t).ToLowerInvariant())
                    select
                        new FileAssociator.DocTypeInfo(string.Format(".{0}", t), description.Substring(description.IndexOf('-') + 2), icon, command, command,
                                                       true)).ToList();
        }

        private static bool IsAdmin
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}