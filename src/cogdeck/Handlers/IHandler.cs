namespace cogdeck.Handlers
{
    internal interface IHandler
    {
        string MenuTitle { get; }
        Task<string> Execute(string input, CancellationToken cancellationToken);
    }
}