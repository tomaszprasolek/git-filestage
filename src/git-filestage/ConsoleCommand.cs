using System;

namespace git_filestage
{
    /// <summary>
    /// Code copied from: https://github.com/terrajobst/git-istage/blob/master/src/git-istage/ConsoleCommand.cs
    /// </summary>
    internal sealed class ConsoleCommand
    {
        private readonly Action _handler;
        private readonly ConsoleKey _key;
        private readonly ConsoleModifiers _modifiers;

        public ConsoleCommand(Action handler, ConsoleKey key)
        {
            _handler = handler;
            _key = key;
            _modifiers = 0;
        }

        public ConsoleCommand(Action handler, ConsoleKey key, ConsoleModifiers modifiers)
        {
            _handler = handler;
            _key = key;
            _modifiers = modifiers;
        }

        public void Execute()
        {
            _handler();
        }

        public bool MatchesKey(ConsoleKeyInfo keyInfo)
        {
            return _key == keyInfo.Key && _modifiers == keyInfo.Modifiers;
        }
    }
}
