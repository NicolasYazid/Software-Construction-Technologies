using System.Windows;

namespace GinRummy.Client.Views
{
    // Visibility of the parts of a screen that depend on its data, such as the empty state of a list or the actions a profile offers.
    // A part that is not shown is collapsed instead of hidden, so that it gives its room back to the rest of the screen.
    internal static class VisibilityCommon
    {
        internal static Visibility FromCondition(bool isVisible)
        {
            Visibility visibility = Visibility.Collapsed;
            if (isVisible)
            {
                visibility = Visibility.Visible;
            }

            return visibility;
        }
    }
}
