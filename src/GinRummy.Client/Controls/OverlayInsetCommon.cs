using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    // The list is padded by the area the cards cover at its top and bottom so the first and last messages can be scrolled clear of them.
    // The padding is measured on every layout pass because a card can grow with its text or appear and disappear.
    public static class OverlayInsetCommon
    {
        private const double Half = 0.5;
        private const double NoRoom = 0.0;
        private const double Tolerance = 0.5;

        public static readonly DependencyProperty IsInsetProperty =
            DependencyProperty.RegisterAttached(
                "IsInset",
                typeof(bool),
                typeof(OverlayInsetCommon),
                new PropertyMetadata(false, OnIsInsetChanged));

        // A static class has no instance state, so each list stores its own handler to be able to unsubscribe it later.
        private static readonly DependencyProperty LayoutHandlerProperty =
            DependencyProperty.RegisterAttached(
                "LayoutHandler",
                typeof(EventHandler),
                typeof(OverlayInsetCommon),
                new PropertyMetadata(null));

        // WPF resolves an attached property from XAML through static Get and Set accessors, so these cannot be a property.
        public static bool GetIsInset(DependencyObject element)
        {
            return (bool)element.GetValue(IsInsetProperty);
        }

        public static void SetIsInset(DependencyObject element, bool value)
        {
            element.SetValue(IsInsetProperty, value);
        }

        // ScrollIntoView is not used because it stops as soon as the newest message shows, leaving it under the bottom card.
        // Scrolling the viewer to the end also reveals the padding kept below that message.
        public static void ScrollToEnd(DependencyObject list)
        {
            ScrollViewer viewer = FindScrollViewer(list);
            if (viewer != null)
            {
                viewer.ScrollToEnd();
            }
        }

        private static void OnIsInsetChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            Control list = element as Control;
            if (list != null)
            {
                EventHandler handler = (EventHandler)list.GetValue(LayoutHandlerProperty);
                if (handler != null)
                {
                    list.LayoutUpdated -= handler;
                    list.ClearValue(LayoutHandlerProperty);
                }

                if ((bool)e.NewValue)
                {
                    handler = (sender, args) => ApplyInset(list);
                    list.SetValue(LayoutHandlerProperty, handler);
                    list.LayoutUpdated += handler;
                }
            }
        }

        // Card margins count as covered area so the gap between a card and the nearest message matches the one the panel leaves.
        private static void ApplyInset(Control list)
        {
            Panel parent = VisualTreeHelper.GetParent(list) as Panel;
            if ((parent != null) && list.IsVisible)
            {
                Rect listBounds = BoundsIn(list, parent);
                double listMiddle = listBounds.Top + (listBounds.Height * Half);
                double top = NoRoom;
                double bottom = NoRoom;

                foreach (UIElement child in parent.Children)
                {
                    FrameworkElement card = child as FrameworkElement;
                    if (IsFloatingCard(card, list))
                    {
                        Rect bounds = BoundsIn(card, parent);
                        double cardTop = bounds.Top - card.Margin.Top;
                        double cardBottom = bounds.Bottom + card.Margin.Bottom;
                        bool isOverlappingList = (cardBottom > listBounds.Top) && (cardTop < listBounds.Bottom);
                        bool isAbove = (bounds.Top + (bounds.Height * Half)) < listMiddle;
                        if (isOverlappingList && isAbove)
                        {
                            top = Math.Max(top, cardBottom - listBounds.Top);
                        }
                        else if (isOverlappingList)
                        {
                            bottom = Math.Max(bottom, listBounds.Bottom - cardTop);
                        }
                    }
                }

                SetRoom(list, top, bottom);
            }
        }

        private static bool IsFloatingCard(FrameworkElement card, Control list)
        {
            return (card != null) && !ReferenceEquals(card, list) && card.IsVisible;
        }

        // The padding is written only when it changes, because each write triggers a new layout pass that runs this measurement again.
        private static void SetRoom(Control list, double top, double bottom)
        {
            Thickness current = list.Padding;
            bool hasChanged = (Math.Abs(current.Top - top) > Tolerance)
                || (Math.Abs(current.Bottom - bottom) > Tolerance);
            if (hasChanged)
            {
                list.Padding = new Thickness(current.Left, top, current.Right, bottom);
            }
        }

        private static Rect BoundsIn(FrameworkElement element, Visual ancestor)
        {
            return element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));
        }

        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            ScrollViewer viewer = root as ScrollViewer;
            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int childIndex = 0; (viewer == null) && (childIndex < childCount); childIndex++)
            {
                viewer = FindScrollViewer(VisualTreeHelper.GetChild(root, childIndex));
            }

            return viewer;
        }
    }
}
