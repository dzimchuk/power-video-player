using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Reflection;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Pvp.App.Util
{
    class SingleInstance : IDisposable
    {
        public delegate void ArgsHandler(string[] args);

        public event ArgsHandler ArgsRecieved;

        private readonly Guid _appGuid;
        private readonly string _assemblyName;

        private Mutex _mutex;
        private bool _owned;
        private Window _window;

        private class Bridge
        {
            public event Action<Guid> BringToFront;
            public event Action<Guid, string[]> ProcessArgs;

            public void OnBringToFront(Guid appGuid)
            {
                if (BringToFront != null)
                    BringToFront(appGuid);
            }

            public void OnProcessArgs(Guid appGuid, string[] args)
            {
                if (ProcessArgs != null)
                    ProcessArgs(appGuid, args);
            }

            private static readonly Bridge _instance = new Bridge();

            static Bridge()
            {
            }

            public static Bridge Instance
            {
                get { return _instance; }
            }
        }

        private class RemotableObject : MarshalByRefObject
        {
            public void BringToFront(Guid appGuid)
            {
                Bridge.Instance.OnBringToFront(appGuid);
            }

            public void ProcessArguments(Guid appGuid, string[] args)
            {
                Bridge.Instance.OnProcessArgs(appGuid, args);
            }
        }

        public SingleInstance(Guid appGuid)
        {
            _appGuid = appGuid;
            _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            Bridge.Instance.BringToFront += BringToFront;
            Bridge.Instance.ProcessArgs += ProcessArgs;

            _mutex = new Mutex(true, _assemblyName + _appGuid, out _owned);
        }

        public void Dispose()
        {
            if (_owned) // always release a mutex if you own it
            {
                _owned = false;
                _mutex.ReleaseMutex();
            }

            Bridge.Instance.BringToFront -= BringToFront;
            Bridge.Instance.ProcessArgs -= ProcessArgs;
        }

        public void Run(Func<Window> showWindow, string[] args)
        {
            if (_owned)
            {
                // show the main app window
                _window = showWindow();
                // and start the service
                StartService();
            }
            else
            {
                SendCommandLineArgs(args);
                Application.Current.Shutdown();
            }
        }

        private void StartService()
        {
            try
            {
                IpcServerChannel channel = new IpcServerChannel("pvp");
                ChannelServices.RegisterChannel(channel, false);

                RemotingConfiguration.RegisterActivatedServiceType(typeof(RemotableObject));
            }
            catch
            {
                // log it
            }
        }

        private void BringToFront(Guid appGuid)
        {
            if (appGuid == _appGuid)
            {
                _window.Dispatcher.BeginInvoke((ThreadStart)delegate()
                    {
                        if (_window.WindowState == WindowState.Minimized)
                            _window.WindowState = WindowState.Normal;
                        _window.Activate();
                    });
            }
        }

        private void ProcessArgs(Guid appGuid, string[] args)
        {
            if (appGuid == _appGuid && ArgsRecieved != null)
            {
                _window.Dispatcher.BeginInvoke((ThreadStart)delegate()
                {
                    ArgsRecieved(args);
                });
            }
        }

        private void SendCommandLineArgs(string[] args)
        {
            try
            {
                IpcClientChannel channel = new IpcClientChannel();
                ChannelServices.RegisterChannel(channel, false);

                RemotingConfiguration.RegisterActivatedClientType(typeof(RemotableObject), "ipc://pvp");

                RemotableObject proxy = new RemotableObject();
                proxy.BringToFront(_appGuid);
                proxy.ProcessArguments(_appGuid, args);
            }
            catch
            { // log it
            }
        }
    }
}
