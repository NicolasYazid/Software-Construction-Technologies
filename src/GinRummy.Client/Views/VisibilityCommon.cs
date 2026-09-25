using System.Windows;

namespace GinRummy.Client.Views
{
    // Visibility of the parts of a screen that depend on its data, such as the empty state of
    // a list or the actions a profile offers. Collapsed and not hidden, so that a part that is
    // not shown gives its room back to the rest of the screen.
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
