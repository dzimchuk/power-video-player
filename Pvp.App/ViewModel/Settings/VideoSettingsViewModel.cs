using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.Settings
{
    internal class VideoSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsProvider _settingsProvider;
        private Renderer _renderer;

        private ICommand _recommendedRendererCommand;

        public VideoSettingsViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            Load();
        }

        private void Load()
        {
            _renderer = RendererOriginal;
        }

        public void Persist()
        {
            _settingsProvider.Set("Renderer", _renderer);
        }

        private Renderer RendererOriginal
        {
            get { return _settingsProvider.Get("Renderer", MediaEngineServiceProvider.RecommendedRenderer); }
        }

        public Renderer Renderer
        {
            get { return _renderer; }
            set
            {
                if (Equals(value, _renderer)) return;
                _renderer = value;
                RaisePropertyChanged("Renderer");
            }
        }

        public ICommand RecommendedRendererCommand
        {
            get
            {
                if (_recommendedRendererCommand == null)
                {
                    _recommendedRendererCommand = new RelayCommand(
                        () =>
                            {
                                Renderer = MediaEngineServiceProvider.RecommendedRenderer;
                            });
                }

                return _recommendedRendererCommand;
            }
        }

        public bool AnyChanges { get { return _renderer != RendererOriginal; } }
    }
}