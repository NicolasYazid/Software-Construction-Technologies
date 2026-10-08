using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    // Lets a list run under the glass cards that float over it in the same panel, so the cards
    // show the messages through their translucent face. The list keeps at its top and at its
    // bottom as much room as the cards cover there, which is what lets the first and the last
    // message be scrolled clear of them. The room is measured from the cards themselves every
    // time the layout changes, so a card that grows with its text, or one that comes and goes,
    // moves the room with it.
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

        // The handler is kept on the list itself, so it can be taken off the same list later.
        private static readonly DependencyProperty LayoutHandlerProperty =
            DependencyProperty.RegisterAttached(
                "LayoutHandler",
                typeof(EventHandler),
                typeof(OverlayInsetCommon),
                new PropertyMetadata(null));

        public static bool GetIsInset(DependencyObject element)
        {
            return (bool)element.GetValue(IsInsetProperty);
        }

        public static void SetIsInset(DependencyObject element, bool value)
        {
            element.SetValue(IsInsetProperty, value);
        }

        // Bringing the newest message into view stops as soon as the message shows, which leaves
        // it under a card at the foot of the list; scrolling to the end shows the room kept below
        // it as well.
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

        // A card counts for the top when its middle lies above the middle of the list, and for
        // the bottom otherwise. The margins of the card belong to the room it takes, so the gap
        // between the card and the first message matches the one the panel already leaves.
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
                    bool isCard = (card != null) && !ReferenceEquals(card, list) && card.IsVisible;
                    if (isCard)
                    {
                        Rect bounds = BoundsIn(card, parent);
                        double cardTop = bounds.Top - card.Margin.Top;
                        double cardBottom = bounds.Bottom + card.Margin.Bottom;
                        bool overlapsList = (cardBottom > listBounds.Top) && (cardTop < listBounds.Bottom);
                        bool isAbove = (bounds.Top + (bounds.Height * Half)) < listMiddle;
                        if (overlapsList && isAbove)
                        {
                            top = Math.Max(top, cardBottom - listBounds.Top);
                        }
                        else if (overlapsList)
                        {
                            bottom = Math.Max(bottom, listBounds.Bottom - cardTop);
                        }
                    }
                }

                SetRoom(list, top, bottom);
            }
        }

        // The room is written only when it changes: writing it asks for a new layout, and the
        // layout that follows measures the same room and leaves it alone.
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
