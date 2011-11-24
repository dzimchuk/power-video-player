﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Dzimchuk.Pvp.App.Util;
using Ninject;
using Dzimchuk.Pvp.App.Composition;

namespace Dzimchuk.Pvp.App
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

            SingleInstance si = new SingleInstance(_appGuid);
            si.ArgsRecieved += new SingleInstance.ArgsHandler(si_ArgsRecieved);
            si.Run(() =>
            {
                SetUpDependencies();
                new MainWindow().Show();
                return this.MainWindow;
            }, e.Args);
        }

        private void si_ArgsRecieved(string[] args)
        {
            if (args.Length > 0)
                this.MainWindow.Content += string.Format("Recieved args: {0}\n", args.Aggregate((all, arg) => all += (" " + arg)));
        }

        private void SetUpDependencies()
        {
            var kernel = new StandardKernel();
            kernel.Load(typeof(App).Assembly);

            var resolver = new NinjectDependencyResolver(kernel);
            DependencyResolver.SetResolver(resolver);
        }
    }
}
