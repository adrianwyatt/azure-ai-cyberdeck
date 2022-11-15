namespace cogdeck.Handlers
{
    internal class RandomHandler : IHandler
    {
        public string MenuTitle => "Random";
        private readonly Random _random = new Random();
        private readonly StatusManager _statusManager;
        public RandomHandler(
            StatusManager statusManager)
        {
            _statusManager = statusManager;
        } 

        public Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            string s = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                s += Convert.ToChar(_random.Next(65, 91));
            }

            _statusManager.Status = "Added random line.";
            if (string.IsNullOrEmpty(input))
            {
                return Task.FromResult(s);
            }
            return Task.FromResult(string.Join(Environment.NewLine, input, s));
            
        }
    }
}
