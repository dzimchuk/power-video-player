using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dzimchuk.MediaEngine.Core;

namespace Dzimchuk.Pvp.App
{
    internal class MediaEngineProvider : IMediaEngineProvider, IMediaEngineProviderSetter
    {
        private IMediaEngine _engine;

        public IMediaEngine MediaEngine
        {
            get { return _engine; }
        }

        IMediaEngine IMediaEngineProviderSetter.MediaEngine
        {
            set { _engine = value; }
        }

        private MediaEngineProvider()
        {
        }

        private static MediaEngineProvider _instance;

        static MediaEngineProvider()
        {
            _instance = new MediaEngineProvider();
        }

        public static MediaEngineProvider Instance
        {
            get { return _instance; }
        }
    }
}
