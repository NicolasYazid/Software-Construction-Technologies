using System;

namespace GinRummy.Client.Controls
{
    public class CardEventArgs : EventArgs
    {
        public CardEventArgs(object item)
        {
            Item = item;
        }

        public object Item { get; private set; }
    }
}
