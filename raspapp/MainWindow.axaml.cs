using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace raspapp
{
    public partial class MainWindow : Window
    {
        private readonly TextBlock Clock;
        private readonly TextBox Names;
        public MainWindow()
        {
            InitializeComponent();

            Clock = this.FindControl<TextBlock>("clock");

            // Update the current time every second
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, UpdateCurrentTime);
            timer.Start();
            //timer.Elapsed += (sender, e) => UpdateCurrentTime();
            //timer.Start();
            Names = this.FindControl<TextBox>("names");

            // Subscribe to the KeyUp event to handle keyboard input
            Names.KeyUp += TextBox_KeyUp;
            Names.Focus();

            this.AttachedToVisualTree += (sender, e) => Names.Focus();
        }


        string enteredLine;
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // User pressed Enter, process the entered line
                var current = Names.Text + Environment.NewLine;
                Names.Text  = current + enteredLine + "  No Name  " + DateTime.Now.ToString("HH:mm:ss");
                enteredLine = "";
                return;
            }
            enteredLine += e.KeySymbol;
        }


        private void UpdateCurrentTime(object sender, EventArgs e)
        {
            Names.Focus();
            // Update the current time property
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");

            // Update the TextBox on the UI thread
            Dispatcher.UIThread.InvokeAsync(() => Clock.Text = $"{CurrentTime}");
        }

        public string CurrentTime
        {
            get { return GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly StyledProperty<string> CurrentTimeProperty =
            AvaloniaProperty.Register<MainWindow, string>(nameof(CurrentTime));

    }
}