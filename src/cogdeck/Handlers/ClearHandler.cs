using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cogdeck.Handlers
{
    internal class ClearHandler : IHandler
    {
        public string MenuTitle => "Clear";
        private readonly StatusManager _statusManager;

        public ClearHandler(
            StatusManager statusManager)
        {
            _statusManager= statusManager;
        }
        public Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            _statusManager.Status = "Cleared workspace.";
            return Task.FromResult(string.Empty);
        }
    }
}
