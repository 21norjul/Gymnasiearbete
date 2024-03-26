using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Globalization;
using System.Threading;

namespace raspapp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            CultureInfo swedishCulture = new CultureInfo("sv-SE");
            Thread.CurrentThread.CurrentCulture = swedishCulture;
            Thread.CurrentThread.CurrentUICulture = swedishCulture;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}