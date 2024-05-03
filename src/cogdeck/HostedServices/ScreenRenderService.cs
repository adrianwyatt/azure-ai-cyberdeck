using System.Reflection;
using System.Text.RegularExpressions;
using cogdeck.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Device.Gpio;
using System.Threading;
using System.Linq.Expressions;

namespace cogdeck.HostedServices
{
    /// <summary>
    /// Hosted service that renders the screen and handles user input.
    /// </summary>
    internal class ScreenRenderService : IHostedService
    {
        private Task _task = Task.CompletedTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly IList<IHandler> _handlers;

        private int titleHeight = 1;
        private int rightScroll = 0;
        private int leftSelect = 0;
        private string rightWorkspace = string.Empty;
        private List<string> rightWorkspaceLines = new List<string>();
        private readonly string title = $"Welcome to Cogdeck v{Assembly.GetEntryAssembly()?.GetName()?.Version?.Major}.{Assembly.GetEntryAssembly()?.GetName()?.Version?.Minor}";
        private readonly StatusManager _statusManager;
        private readonly ILogger _logger;
        private readonly GpioController? _gpioController = null;

        private readonly Queue<ConsoleKey> _inputQueue = new Queue<ConsoleKey>();
        private readonly SemaphoreSlim _inputQueueSignal = new SemaphoreSlim(1, 1);

        public ScreenRenderService(
            IEnumerable<IHandler> handlers,
            StatusManager statusManager,
            ILogger<ScreenRenderService> logger)
        {
            _handlers = handlers.ToList();
            _statusManager = statusManager;
            _logger = logger;

            if (!OperatingSystem.IsWindows())
            {
                _gpioController = new GpioController();
            }
        }
        /// <summary>
        /// Starts the screen render service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!OperatingSystem.IsWindows())
            {
                _gpioController?.OpenPin(2, PinMode.InputPullUp);
                _gpioController?.RegisterCallbackForPinValueChangedEvent(2, PinEventTypes.Falling, (sender, args) => { _inputQueue.Enqueue(ConsoleKey.Enter); _inputQueueSignal.Release(); }); // HandleInputKey(ConsoleKey.Enter, cancellationToken).Wait());
                _gpioController?.OpenPin(3, PinMode.InputPullUp);
                _gpioController?.RegisterCallbackForPinValueChangedEvent(3, PinEventTypes.Falling, (sender, args) => { _inputQueue.Enqueue(ConsoleKey.W); _inputQueueSignal.Release(); }); //  HandleInputKey(ConsoleKey.W, cancellationToken).Wait());
                _gpioController?.OpenPin(4, PinMode.InputPullUp);
                _gpioController?.RegisterCallbackForPinValueChangedEvent(4, PinEventTypes.Falling, (sender, args) => { _inputQueue.Enqueue(ConsoleKey.S); _inputQueueSignal.Release(); }); //  HandleInputKey(ConsoleKey.S, cancellationToken).Wait());
            }

            Task keyTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    _inputQueue.Enqueue(key.Key);
                    _inputQueueSignal.Release();
                }
            });

            _task = ExecuteAsync(_cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the screen render service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return _task;
        }

        /// <summary>
        /// Executes the screen render service.
        /// </summary>
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _statusManager.Status = "Hello.";
            bool keepGoing = true;
            while (keepGoing)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Clear();

                RenderTitle();
                RenderSplitLines();
                RenderLeftMenu();
                RenderRightWorkspace();
                _statusManager.RenderStatus();
                await _inputQueueSignal.WaitAsync();
                while (_inputQueue.Count > 0)
                {
                    await HandleInputKey(_inputQueue.Dequeue(), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Renders the title of the application.
        /// </summary>
        private void RenderTitle()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < title.Length; i++)
            {

                Console.ForegroundColor = (ConsoleColor)((i % 7) + 9);
                Console.Write(title[i]);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        /// <summary>
        /// Renders the split lines between the left and right workspace.
        /// </summary>
        private void RenderSplitLines()
        {
            // Draw the top horizontal line
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                Console.SetCursorPosition(i, titleHeight);
                Console.Write('-');
            }

            // Draw vertical line
            for (int i = titleHeight + 1; i < Console.BufferHeight - 2; i++)
            {
                Console.SetCursorPosition((Console.BufferWidth / 2), i);
                Console.Write('|');
            }

            // Draw the bottom horizontal line
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                Console.SetCursorPosition(i, Console.BufferHeight - 2);
                Console.Write('-');
            }
        }

        private async Task HandleInputKey(ConsoleKey key, CancellationToken cancellationToken)
        {
            switch (key)
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
                    try
                    {
                        rightWorkspace = await _handlers[leftSelect].Execute(rightWorkspace, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _statusManager.Status = $"Oh no! {ex.GetType().Name}:{ex.Message}".Replace("\n", "").Substring(0, Console.BufferWidth - 5) + "...";
                        _logger.LogError(exception: ex, null);
                    }
                    break;
            }
        }

        /// <summary>
        /// Word wraps the given text to the given width.
        /// </summary>
        private IEnumerable<string> WordWrap(string text, int width)
        {
            const string forcedBreakZonePattern = @"\n";
            const string normalBreakZonePattern = @"\s+|(?<=[-,.;])|$";

            var forcedZones = Regex.Matches(text, forcedBreakZonePattern).Cast<Match>().ToList();
            var normalZones = Regex.Matches(text, normalBreakZonePattern).Cast<Match>().ToList();

            int start = 0;

            while (start < text.Length)
            {
                var zone =
                    forcedZones.Find(z => z.Index >= start && z.Index <= start + width) ??
                    normalZones.FindLast(z => z.Index >= start && z.Index <= start + width);

                if (zone == null)
                {
                    yield return text.Substring(start, width);
                    start += width;
                }
                else
                {
                    yield return text.Substring(start, zone.Index - start);
                    start = zone.Index + zone.Length;
                }
            }
        }

        /// <summary>
        /// Renders the text in the right-side workspace.
        /// </summary>
        private void RenderRightWorkspace()
        {
            rightWorkspaceLines = WordWrap(rightWorkspace, Console.BufferWidth / 2 - 2).ToList();

            int maxLines = Console.BufferHeight - 4;
            int workspaceLine = rightScroll;

            for (int line = 0; line < maxLines && workspaceLine < rightWorkspaceLines.Count; line++)
            {
                Console.SetCursorPosition(Console.BufferWidth / 2 + 2, line + titleHeight + 1);
                Console.WriteLine(rightWorkspaceLines[workspaceLine++]);
            }
        }

        /// <summary>
        /// Renders the left-side menu text.
        /// </summary>
        private void RenderLeftMenu()
        {
            int line = 0;
            foreach (IHandler handler in _handlers)
            {
                Console.SetCursorPosition(0, line + titleHeight + 1);
                if (line == leftSelect)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine(handler.MenuTitle);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.BackgroundColor = ConsoleColor.Black;

                line++;
            }
        }
    }
}
