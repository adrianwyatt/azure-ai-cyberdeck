using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cogdeck.Handlers
{
    internal class Echo2Handler : IHandler
    {
        public string MenuTitle => "Echo2";

        public Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            string output = input + Environment.NewLine + "echo2";
            return Task.FromResult(output);
        }
    }
}
