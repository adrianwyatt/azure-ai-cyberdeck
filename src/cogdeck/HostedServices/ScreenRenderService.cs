using cogdeck.Handlers;
using Microsoft.Extensions.Hosting;

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
        private string[] rightWorkspaceLines;

        public ScreenRenderService(IEnumerable<IHandler> handlers)
        {
            _handlers= handlers.ToList();
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
            bool keepGoing = true;
            while (keepGoing)
            {
                rightWorkspaceLines = rightWorkspace.Split(Environment.NewLine);
                RenderSplitLines();
                RenderLeftMenu();
                RenderRightWorkspace();
                await HandleInput(cancellationToken);
            }
        }

        private static void RenderSplitLines()
        {
            for (int i = 0; i < Console.BufferHeight - 2; i++)
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
                    if ()
                    break;
                case ConsoleKey.UpArrow:
                   
                    break;
                case ConsoleKey.Enter:
                    rightWorkspace = await _handlers[leftSelect].Execute(rightWorkspace, cancellationToken);
                    break;

            }
        }

        private void RenderRightWorkspace()
        {
            int line = 0;
            for (int i = rightScroll; i < Math.Min(rightWorkspaceLines.Length, Console.BufferHeight - 2) + rightScroll; i++)
            {
                Console.SetCursorPosition(Console.BufferWidth / 2 + 2, line);
                Console.WriteLine(rightWorkspaceLines[i]);
                line++;
            }
        }

        private void RenderLeftMenu()
        {
            int line = 0;
            foreach (IHandler handler in _handlers)
            {
                Console.SetCursorPosition(0, line);
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
