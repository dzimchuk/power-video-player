using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using res=Pvp.App.Resources;

namespace Pvp.App.Controls
{
    [TemplatePart(Name = "PART_PlayButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_PauseButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_StopButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ForwardButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_BackwardButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ToEndButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ToBeginingButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_RepeatButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_MuteButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_SeekSlider", Type = typeof(CommandSlider))]
    [TemplatePart(Name = "PART_VolumeSlider", Type = typeof(CommandSlider))]
    [TemplatePart(Name = "PART_CurrentPosition", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_Duration", Type = typeof(TextBlock))]
    public abstract class ControlPanelBase : Control
    {
        public static readonly DependencyProperty PlayCommandProperty = 
            DependencyProperty.Register("PlayCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty PauseCommandProperty =
            DependencyProperty.Register("PauseCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty StopCommandProperty =
            DependencyProperty.Register("StopCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty ForwardCommandProperty =
            DependencyProperty.Register("ForwardCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty BackwardCommandProperty =
            DependencyProperty.Register("BackwardCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty ToEndCommandProperty =
            DependencyProperty.Register("ToEndCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty ToBeginingCommandProperty =
            DependencyProperty.Register("ToBeginingCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty RepeatCommandProperty =
            DependencyProperty.Register("RepeatCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty MuteCommandProperty =
            DependencyProperty.Register("MuteCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty SeekCommandProperty =
            DependencyProperty.Register("SeekCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty VolumeCommandProperty =
            DependencyProperty.Register("VolumeCommand",
                                        typeof(ICommand),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata((ICommand)null));

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration",
                                        typeof(TimeSpan),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register("CurrentPosition",
                                        typeof(TimeSpan),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty IsRepeatProperty =
            DependencyProperty.Register("IsRepeat",
                                        typeof(bool),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata(false));

        public static readonly DependencyProperty IsMuteProperty =
            DependencyProperty.Register("IsMute",
                                        typeof(bool),
                                        typeof(ControlPanelBase),
                                        new PropertyMetadata(false));
        
        public ICommand PlayCommand
        {
            get
            {
                return (ICommand)GetValue(PlayCommandProperty);
            }
            set
            {
                SetValue(PlayCommandProperty, value);
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return (ICommand)GetValue(PauseCommandProperty);
            }
            set
            {
                SetValue(PauseCommandProperty, value);
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return (ICommand)GetValue(StopCommandProperty);
            }
            set
            {
                SetValue(StopCommandProperty, value);
            }
        }

        public ICommand ForwardCommand
        {
            get
            {
                return (ICommand)GetValue(ForwardCommandProperty);
            }
            set
            {
                SetValue(ForwardCommandProperty, value);
            }
        }

        public ICommand BackwardCommand
        {
            get
            {
                return (ICommand)GetValue(BackwardCommandProperty);
            }
            set
            {
                SetValue(BackwardCommandProperty, value);
            }
        }

        public ICommand ToEndCommand
        {
            get
            {
                return (ICommand)GetValue(ToEndCommandProperty);
            }
            set
            {
                SetValue(ToEndCommandProperty, value);
            }
        }

        public ICommand ToBeginingCommand
        {
            get
            {
                return (ICommand)GetValue(ToBeginingCommandProperty);
            }
            set
            {
                SetValue(ToBeginingCommandProperty, value);
            }
        }

        public ICommand RepeatCommand
        {
            get
            {
                return (ICommand)GetValue(RepeatCommandProperty);
            }
            set
            {
                SetValue(RepeatCommandProperty, value);
            }
        }

        public ICommand MuteCommand
        {
            get
            {
                return (ICommand)GetValue(MuteCommandProperty);
            }
            set
            {
                SetValue(MuteCommandProperty, value);
            }
        }

        public ICommand SeekCommand
        {
            get
            {
                return (ICommand)GetValue(SeekCommandProperty);
            }
            set
            {
                SetValue(SeekCommandProperty, value);
            }
        }

        public ICommand VolumeCommand
        {
            get
            {
                return (ICommand)GetValue(VolumeCommandProperty);
            }
            set
            {
                SetValue(VolumeCommandProperty, value);
            }
        }

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public TimeSpan CurrentPosition
        {
            get { return (TimeSpan)GetValue(CurrentPositionProperty); }
            set { SetValue(CurrentPositionProperty, value); }
        }

        public bool IsRepeat
        {
            get { return (bool)GetValue(IsRepeatProperty); }
            set { SetValue(IsRepeatProperty, value); }
        }

        public bool IsMute
        {
            get { return (bool)GetValue(IsMuteProperty); }
            set { SetValue(IsMuteProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var playButton = Template.FindName("PART_PlayButton", this) as ButtonBase;
            if (playButton != null)
            {
                Binding binding = new Binding("PlayCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                playButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var pauseButton = Template.FindName("PART_PauseButton", this) as ButtonBase;
            if (pauseButton != null)
            {
                Binding binding = new Binding("PauseCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                pauseButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var stopButton = Template.FindName("PART_StopButton", this) as ButtonBase;
            if (stopButton != null)
            {
                Binding binding = new Binding("StopCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                stopButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var forwardButton = Template.FindName("PART_ForwardButton", this) as ButtonBase;
            if (forwardButton != null)
            {
                Binding binding = new Binding("ForwardCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                forwardButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var backwardButton = Template.FindName("PART_BackwardButton", this) as ButtonBase;
            if (backwardButton != null)
            {
                Binding binding = new Binding("BackwardCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                backwardButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var toEndButton = Template.FindName("PART_ToEndButton", this) as ButtonBase;
            if (toEndButton != null)
            {
                Binding binding = new Binding("ToEndCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                toEndButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var toBeginingButton = Template.FindName("PART_ToBeginingButton", this) as ButtonBase;
            if (toBeginingButton != null)
            {
                Binding binding = new Binding("ToBeginingCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                toBeginingButton.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var repeatButton = Template.FindName("PART_RepeatButton", this) as ToggleButton;
            if (repeatButton != null)
            {
                Binding binding = new Binding("RepeatCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                repeatButton.SetBinding(ButtonBase.CommandProperty, binding);

                binding = new Binding("IsRepeat");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                repeatButton.SetBinding(ToggleButton.IsCheckedProperty, binding);
            }

            var muteButton = Template.FindName("PART_MuteButton", this) as ToggleButton;
            if (muteButton != null)
            {
                Binding binding = new Binding("MuteCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                muteButton.SetBinding(ButtonBase.CommandProperty, binding);

                binding = new Binding("IsMute");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                muteButton.SetBinding(ToggleButton.IsCheckedProperty, binding);
            }

            var seekSlider = Template.FindName("PART_SeekSlider", this) as CommandSlider;
            if (seekSlider != null)
            {
                Binding binding = new Binding("SeekCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                seekSlider.SetBinding(CommandSlider.CommandProperty, binding);
            }

            var volumeSlider = Template.FindName("PART_VolumeSlider", this) as CommandSlider;
            if (volumeSlider != null)
            {
                Binding binding = new Binding("VolumeCommand");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                volumeSlider.SetBinding(CommandSlider.CommandProperty, binding);
            }

            var currentPosition = Template.FindName("PART_CurrentPosition", this) as TextBlock;
            if (currentPosition != null)
            {
                Binding binding = new Binding("CurrentPosition");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                binding.Converter = new TimeSpanValueConverter();
                currentPosition.SetBinding(TextBlock.TextProperty, binding);
            }

            var duration = Template.FindName("PART_Duration", this) as TextBlock;
            if (duration != null)
            {
                Binding binding = new Binding("Duration");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                binding.Converter = new TimeSpanValueConverter();
                duration.SetBinding(TextBlock.TextProperty, binding);
            }

            UpdateTooltips();
        }

        private void UpdateTooltips()
        {
            var playButton = Template.FindName("PART_PlayButton", this) as ButtonBase;
            if (playButton != null)
            {
                playButton.ToolTip = res.Resources.controlbar_play;
            }

            var pauseButton = Template.FindName("PART_PauseButton", this) as ButtonBase;
            if (pauseButton != null)
            {
                pauseButton.ToolTip = res.Resources.controlbar_pause;
            }

            var stopButton = Template.FindName("PART_StopButton", this) as ButtonBase;
            if (stopButton != null)
            {
                stopButton.ToolTip = res.Resources.controlbar_stop;
            }

            var forwardButton = Template.FindName("PART_ForwardButton", this) as ButtonBase;
            if (forwardButton != null)
            {
                forwardButton.ToolTip = res.Resources.controlbar_forward;
            }

            var backwardButton = Template.FindName("PART_BackwardButton", this) as ButtonBase;
            if (backwardButton != null)
            {
                backwardButton.ToolTip = res.Resources.controlbar_backward;
            }

            var toEndButton = Template.FindName("PART_ToEndButton", this) as ButtonBase;
            if (toEndButton != null)
            {
                toEndButton.ToolTip = res.Resources.controlbar_to_end;
            }

            var toBeginingButton = Template.FindName("PART_ToBeginingButton", this) as ButtonBase;
            if (toBeginingButton != null)
            {
                toBeginingButton.ToolTip = res.Resources.controlbar_to_begining;
            }

            var repeatButton = Template.FindName("PART_RepeatButton", this) as ToggleButton;
            if (repeatButton != null)
            {
                repeatButton.ToolTip = res.Resources.controlbar_repeat;
            }

            var muteButton = Template.FindName("PART_MuteButton", this) as ToggleButton;
            if (muteButton != null)
            {
                muteButton.ToolTip = res.Resources.controlbar_mute;
            }

            var seekSlider = Template.FindName("PART_SeekSlider", this) as CommandSlider;
            if (seekSlider != null)
            {
                seekSlider.ToolTip = res.Resources.controlbar_seekbar;
            }

            var volumeSlider = Template.FindName("PART_VolumeSlider", this) as CommandSlider;
            if (volumeSlider != null)
            {
                volumeSlider.ToolTip = res.Resources.controlbar_volume;
            }
        }
    }
}
