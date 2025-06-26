using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;

namespace RaiManager;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                // This disables GPU rendering. For some reason the app started dying and doing this fixes it.
                AllowEglInitialization = false,
            })
            .LogToTrace()
            .UseReactiveUI();
}