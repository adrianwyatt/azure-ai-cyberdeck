using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cogdeck.Handlers
{
    internal class RandomHandler : IHandler
    {
        public string MenuTitle => "Random";
        private readonly Random _random = new Random();

        public Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            for (int i = 0; i < 8; i++)
            {

            }
            Convert.ToChar(_random.Next(0, 26) + 65);
            string output = input + Environment.NewLine + "echo";
            return Task.FromResult(output);
        }
    }
}
