using cogdeck.Handlers;
using Microsoft.Extensions.Hosting;
using System.Linq.Expressions;
using System.Reflection;

namespace cogdeck.HostedServices
{
    internal class ScreenRenderService : IHostedService
    {
        private Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly IList<IHandler> _handlers;

        private int rightScroll = 0;
        private int leftSelect = 0;
        private string rightWorkspace = string.Empty;
        private List<string> rightWorkspaceLines = new List<string>();
        private readonly string title = $"Welcome to Cogdeck v{Assembly.GetEntryAssembly().GetName().Version.Major}.{Assembly.GetEntryAssembly().GetName().Version.Minor}";
        private readonly StatusManager _statusManager;

        public ScreenRenderService(
            IEnumerable<IHandler> handlers,
            StatusManager statusManager)
        {
            _handlers= handlers.ToList();
            _statusManager = statusManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _task = ExecuteAsync(_cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return _task;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _statusManager.Status = "Hello.";
            bool keepGoing = true;
            while (keepGoing)
            {
                Console.Clear();
                
                RenderTitle();
                RenderSplitLines();
                RenderLeftMenu();
                RenderRightWorkspace();
                _statusManager.RenderStatus();
                await HandleInput(cancellationToken);
            }
        }

        private void RenderTitle()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(title);
        }

        private static void RenderSplitLines()
        {
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                Console.SetCursorPosition(i, 1);
                Console.Write('-');
            }
            for (int i = 2; i < Console.BufferHeight - 2; i++)
            {
                Console.SetCursorPosition((Console.BufferWidth / 2), i);
                Console.Write('|');
            }
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                Console.SetCursorPosition(i, Console.BufferHeight - 2);
                Console.Write('-');
            }
        }

        private async Task HandleInput(CancellationToken cancellationToken)
        {
            Console.SetCursorPosition(Console.BufferWidth - 1, Console.BufferHeight - 1);
            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.W:
                    if (leftSelect > 0) leftSelect--;
                    break;
                case ConsoleKey.S:
                    if (leftSelect < _handlers.Count - 1) leftSelect++;
                    break;
                case ConsoleKey.DownArrow:
                    if (rightScroll < rightWorkspaceLines.Count) rightScroll++;
                    break;
                case ConsoleKey.UpArrow:
                    if (rightScroll > 0) rightScroll--;
                    break;
                case ConsoleKey.Enter:
                    rightWorkspace = await _handlers[leftSelect].Execute(rightWorkspace, cancellationToken);
                    break;

            }
        }

        

        private void RenderRightWorkspace()
        {
            // first split by obvious newlines
            //rightWorkspaceLines = rightWorkspace.Split(Environment.NewLine).ToList();
            // then do word wrap
            for (int i = 0; i < rightWorkspaceLines.Count; i++)

            int line = 0;
            int maxLines = Console.BufferHeight - 4;
            int workspaceLine = rightScroll;

            for (; line < maxLines && workspaceLine < rightWorkspaceLines.Count; line++)
            {
                Console.SetCursorPosition(Console.BufferWidth / 2 + 2, line + 2);
                Console.WriteLine(rightWorkspaceLines[workspaceLine++]);
            }
        }

        private void RenderLeftMenu()
        {
            int line = 0;
            foreach (IHandler handler in _handlers)
            {
                Console.SetCursorPosition(0, line + 2);
                if (line == leftSelect)
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                }
                Console.WriteLine(handler.MenuTitle);
                Console.BackgroundColor = ConsoleColor.Black;
                line++;
            }
        }
    }
}
