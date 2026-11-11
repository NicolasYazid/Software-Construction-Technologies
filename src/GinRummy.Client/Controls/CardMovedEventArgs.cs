using System;

namespace GinRummy.Client.Controls
{
    public class CardMovedEventArgs : EventArgs
    {
        public CardMovedEventArgs(int oldIndex, int newIndex)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }
    }
}
