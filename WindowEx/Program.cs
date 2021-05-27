using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using static PInvoke.User32;

namespace WindowEx
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var borderCommand = new Command("border")
            {
                new Argument<bool>("enableBorder"),
                new Option<string?>(new[] { "--window-title", "-t" })
            };
            borderCommand.Handler = CommandHandler.Create<bool, string?, int?, IConsole>(SetBorder);
            RootCommand rootCommand = new()
            {
                borderCommand
            };

            await rootCommand.InvokeAsync(args);
        }

        private static void SetBorder(
            bool enableBorder,
            string? windowTitle,
            int? processId,
            IConsole console)
        {
            if (string.IsNullOrWhiteSpace(windowTitle) && processId == null)
            {
                console.Out.WriteLine("Must specify a window title or process id");
                return;
            }
            if (!string.IsNullOrWhiteSpace(windowTitle) && processId != null)
            {
                console.Out.WriteLine("Must only specify window title or process id");
                return;
            }

            EnumWindows(new WNDENUMPROC(WindowCallback), IntPtr.Zero);

            bool WindowCallback(IntPtr hwnd, IntPtr lParam)
            {
                string windowText = GetWindowText(hwnd);
                if (!string.IsNullOrEmpty(windowTitle) &&
                    string.Equals(windowTitle, GetWindowText(hwnd)))
                {
                    int windowStyle = GetWindowLong(hwnd, WindowLongIndexFlags.GWL_STYLE);
                    if (enableBorder)
                    {
                        windowStyle |= (int)SetWindowLongFlags.WS_BORDER;
                        windowStyle |= (int)SetWindowLongFlags.WS_THICKFRAME;
                    }
                    else
                    {
                        windowStyle &= ~(int)SetWindowLongFlags.WS_BORDER;
                        windowStyle &= ~(int)SetWindowLongFlags.WS_THICKFRAME;
                    }
                    SetWindowLong(hwnd, WindowLongIndexFlags.GWL_STYLE, (SetWindowLongFlags)windowStyle);
                    return false;
                }

                return true;
            }
        }
    }
}
