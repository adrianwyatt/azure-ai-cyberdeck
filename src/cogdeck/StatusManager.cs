namespace cogdeck
{
    internal class StatusManager
    {
        private string _status = string.Empty;
        public string Status
        {
            get { return _status; }
            set
            {
                ClearStatus();
                _status = value;
                RenderStatus();
            }
        }

        public void RenderStatus()
        {
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            Console.Write(_status);
        }

        private void ClearStatus()
        {
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            for (int i = 0; i < _status.Length; i++)
            {
                Console.Write(' ');
            }
        }
    }
}
