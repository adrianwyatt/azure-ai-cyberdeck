namespace cogdeck.Handlers
{
    internal interface IHandler
    {
        Task<string> Execute(string input, CancellationToken cancellationToken);
    }
}