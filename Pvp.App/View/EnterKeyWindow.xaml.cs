using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.App.ViewModel;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for EnterKeyWindow.xaml
    /// </summary>
    public partial class EnterKeyWindow : Window
    {
        public static readonly DependencyProperty SelectedKeyCombinationProperty =
            DependencyProperty.Register("SelectedKeyCombination", typeof(KeyCombination), typeof(EnterKeyWindow), 
            new PropertyMetadata(default(KeyCombination)));

        public EnterKeyWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CommandMessage>(this, OnCommand);

            var binding = new Binding("SelectedKeyCombination");
            binding.Source = DataContext;
            binding.Mode = BindingMode.OneWay;
            SetBinding(SelectedKeyCombinationProperty, binding);
        }

        private void OnCommand(CommandMessage message)
        {
            if (message.Content == Command.EnterKeyWindowClose)
            {
                Close();
            }
        }

        public KeyCombination SelectedKeyCombination
        {
            get { return (KeyCombination)GetValue(SelectedKeyCombinationProperty); }
        }
    }
}