using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using GinRummy.Client.Effects;

namespace GinRummy.Client.Controls
{
    // The hand moves every card through its own transform so that each card can follow the pointer, sway and tilt.
    // A ListBox lays its items out by itself and drags them with the drag and drop of the system, which cannot animate a card.
    public class CardHand : ItemsControl
    {
        private const double DefaultTiltRoom = 24.0;
        private const double SidesPerAxis = 2.0;
        private const double Half = 0.5;
        private const double MillisecondsPerSecond = 1000.0;
        private const double MaxCatchUpSeconds = 0.25;
        private const int DraggedLayer = 1000;
        private const int FrontFace = 0;
        private const int BackFace = 1;

        // A chosen card rises as far as the ListBox used to raise it.
        private const double SelectedLift = 14.0;

        private const double DefaultEntryDrop = 40.0;

        // Consecutive cards start their sway this many radians apart.
        private const double PhaseStep = 0.9;

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CardHand),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // The room is transparent space around each card where a tilted card can grow without being cut.
        public static readonly DependencyProperty TiltRoomProperty = DependencyProperty.Register(
            nameof(TiltRoom),
            typeof(double),
            typeof(CardHand),
            new FrameworkPropertyMetadata(DefaultTiltRoom));

        // A card that appears in the hand without coming from elsewhere starts this far below its place and rises into it.
        // A pile of the table sets it to zero, so a card that lands on it is not seen rising a second time.
        public static readonly DependencyProperty EntryDropProperty = DependencyProperty.Register(
            nameof(EntryDrop),
            typeof(double),
            typeof(CardHand),
            new FrameworkPropertyMetadata(DefaultEntryDrop));

        // A read-only hand, such as the one of the opponent, floats and answers the pointer but cannot be chosen or dragged.
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(CardHand),
            new FrameworkPropertyMetadata(false));

        // The tilt invites the player to pick the card up, so a hand that cannot be touched may turn it off.
        public static readonly DependencyProperty IsHoverTiltEnabledProperty = DependencyProperty.Register(
            nameof(IsHoverTiltEnabled),
            typeof(bool),
            typeof(CardHand),
            new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsFanFlippedProperty = DependencyProperty.Register(
            nameof(IsFanFlipped),
            typeof(bool),
            typeof(CardHand),
            new FrameworkPropertyMetadata(false));

        // The back is shown while a card that entered face down is still turning over.
        public static readonly DependencyProperty BackTemplateProperty = DependencyProperty.Register(
            nameof(BackTemplate),
            typeof(DataTemplate),
            typeof(CardHand),
            new FrameworkPropertyMetadata(null));

        // A card released over this element is offered to the listener of CardDropped instead of being reordered.
        public static readonly DependencyProperty DropTargetProperty = DependencyProperty.Register(
            nameof(DropTarget),
            typeof(FrameworkElement),
            typeof(CardHand),
            new FrameworkPropertyMetadata(null));

        // A pile lets its card be dragged only towards the drop target, and the card is never chosen nor reordered.
        // Released anywhere else, the card goes back to its place.
        public static readonly DependencyProperty IsDragOnlyProperty = DependencyProperty.Register(
            nameof(IsDragOnly),
            typeof(bool),
            typeof(CardHand),
            new FrameworkPropertyMetadata(false));

        private static readonly DependencyPropertyKey IsDraggingPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsDragging),
            typeof(bool),
            typeof(CardHand),
            new FrameworkPropertyMetadata(false));

        // A dragged card can travel over the elements laid out after the hand, so the screen lifts the hand above them meanwhile.
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        private readonly Dictionary<object, CardMotion> _motions = new Dictionary<object, CardMotion>();
        private readonly Dictionary<object, Point> _exitTargets = new Dictionary<object, Point>();
        private readonly HashSet<object> _departedItems = new HashSet<object>();
        private readonly List<object> _arrivedItems = new List<object>();
        private readonly List<object> _order = new List<object>();
        private TimeSpan _lastRenderingTime;
        private double _pendingSeconds;
        private bool _isSubscribedToRendering;
        private int _createdCount;
        private Point _pointer;
        private bool _isPointerInside;
        private object _hoveredItem;
        private object _pressedItem;
        private object _draggedItem;
        private Point _pressPoint;
        private int _dragStartIndex;
        private Point _lastDragPoint;
        private int _lastDragTimestamp;

        // The transparent background lets the gaps between the cards receive the pointer too.
        public CardHand()
        {
            Background = Brushes.Transparent;
            ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas)));
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public event EventHandler<CardMovedEventArgs> CardMoved;

        public event EventHandler<CardEventArgs> CardDropped;

        public event EventHandler<CardEventArgs> CardLeft;

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public double TiltRoom
        {
            get { return (double)GetValue(TiltRoomProperty); }
            set { SetValue(TiltRoomProperty, value); }
        }

        public double EntryDrop
        {
            get { return (double)GetValue(EntryDropProperty); }
            set { SetValue(EntryDropProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public bool IsHoverTiltEnabled
        {
            get { return (bool)GetValue(IsHoverTiltEnabledProperty); }
            set { SetValue(IsHoverTiltEnabledProperty, value); }
        }

        public bool IsFanFlipped
        {
            get { return (bool)GetValue(IsFanFlippedProperty); }
            set { SetValue(IsFanFlippedProperty, value); }
        }

        public DataTemplate BackTemplate
        {
            get { return (DataTemplate)GetValue(BackTemplateProperty); }
            set { SetValue(BackTemplateProperty, value); }
        }

        public FrameworkElement DropTarget
        {
            get { return (FrameworkElement)GetValue(DropTargetProperty); }
            set { SetValue(DropTargetProperty, value); }
        }

        public bool IsDragOnly
        {
            get { return (bool)GetValue(IsDragOnlyProperty); }
            set { SetValue(IsDragOnlyProperty, value); }
        }

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            private set { SetValue(IsDraggingPropertyKey, value); }
        }

        // The card starts where the source is drawn and at its size, and then travels to its place in the hand.
        // The caller announces the card before adding it to the items, so the card never appears in its place first.
        public void EnterFrom(object item, FrameworkElement source, bool isFaceDown)
        {
            Point sourceCenter = new Point(source.ActualWidth * Half, source.ActualHeight * Half);
            CardMotion motion = CreateMotion(source.TranslatePoint(sourceCenter, this));
            motion.PrepareEntry(GetScaleFrom(source.ActualHeight), isFaceDown);
            _motions[item] = motion;
        }

        // A card dragged out of another hand, such as the discard pile, goes on from where that hand draws it and at the size it has there.
        public void EnterFrom(object item, CardHand source)
        {
            if (!source._motions.TryGetValue(item, out CardMotion sourceMotion))
            {
                EnterFrom(item, source, false);
                return;
            }

            double sourceHeight = source.GetCardSize().Height * sourceMotion.Scale;
            CardMotion motion = CreateMotion(source.TranslatePoint(sourceMotion.Position, this));
            motion.PrepareEntry(GetScaleFrom(sourceHeight), false);
            _motions[item] = motion;
        }

        // The card leaves its place at once, so the rest of the hand closes the gap while it travels.
        // CardLeft is raised when it lands, and the listener then removes it from the items.
        public void SendTo(object item, FrameworkElement target)
        {
            if (!_motions.TryGetValue(item, out CardMotion motion))
            {
                return;
            }

            Point targetCenter = new Point(target.ActualWidth * Half, target.ActualHeight * Half);
            Size cardSize = GetCardSize();
            double exitScale = 1;
            if (cardSize.Height > 0)
            {
                exitScale = target.ActualHeight / cardSize.Height;
            }

            motion.PrepareExit(exitScale);
            _exitTargets[item] = target.TranslatePoint(targetCenter, this);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            Grid faces = new Grid();
            faces.Children.Add(new ContentPresenter());
            faces.Children.Add(new ContentPresenter { Visibility = Visibility.Collapsed });
            Border host = new Border();
            host.Background = Brushes.Transparent;
            host.RenderTransform = new MatrixTransform();
            host.Child = faces;

            return host;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return false;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            Border host = (Border)element;
            host.Padding = new Thickness(TiltRoom);
            ContentPresenter front = GetFace(host, FrontFace);
            front.Content = item;
            front.ContentTemplate = ItemTemplate;
            ContentPresenter back = GetFace(host, BackFace);
            back.Content = item;
            back.ContentTemplate = BackTemplate;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            SynchronizeOrder();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _pointer = e.GetPosition(this);
            _isPointerInside = true;
            if ((_pressedItem != null) && HasLeftClickArea(_pointer))
            {
                StartDrag(e.Timestamp);
            }

            if (_draggedItem != null)
            {
                TrackDrag(e.Timestamp);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _isPointerInside = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsReadOnly)
            {
                return;
            }

            _pointer = e.GetPosition(this);
            _pressedItem = FindItemAt(_pointer);
            if (_pressedItem != null)
            {
                _pressPoint = _pointer;
                CaptureMouse();
                e.Handled = true;
            }
        }

        // A short press chooses the card and only a longer movement drags it (house rule 18).
        // A card of a pile cannot be chosen, so a short press on it does nothing.
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_draggedItem != null)
            {
                FinishDrag();
            }
            else if ((_pressedItem != null) && !IsDragOnly)
            {
                SelectedItem = _pressedItem;
            }

            _pressedItem = null;
            ReleaseMouseCapture();
        }

        // The capture can be lost without a release, for example when another window takes the focus in the middle of a drag.
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            _pressedItem = null;
            if (_draggedItem != null)
            {
                FinishDrag();
            }
        }

        // Consecutive cards start their sway apart, so each new motion takes the next phase.
        private CardMotion CreateMotion(Point position)
        {
            CardMotion motion = new CardMotion(position, _createdCount * PhaseStep);
            _createdCount++;

            return motion;
        }

        // The scale at which a card of this hand is drawn as tall as the given height.
        private double GetScaleFrom(double height)
        {
            Size cardSize = GetCardSize();
            double scale = 1;
            if (cardSize.Height > 0)
            {
                scale = height / cardSize.Height;
            }

            return scale;
        }

        private static ContentPresenter GetFace(Border host, int face)
        {
            return (ContentPresenter)((Grid)host.Child).Children[face];
        }

        // The face is hidden and not collapsed, so the card keeps its size while its back is shown.
        private void ShowFace(Border host, bool isShowingBack)
        {
            bool canShowBack = isShowingBack && (BackTemplate != null);
            GetFace(host, FrontFace).Visibility = canShowBack ? Visibility.Hidden : Visibility.Visible;
            GetFace(host, BackFace).Visibility = canShowBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void ApplyTilt(Border host, CardTilt tilt, Size cardSize)
        {
            // The shader is only attached while the card leans, because a flat card looks right without it.
            if (!tilt.IsTilted)
            {
                host.Effect = null;
                return;
            }

            CardTiltEffect effect = host.Effect as CardTiltEffect;
            if (effect == null)
            {
                effect = new CardTiltEffect();
                host.Effect = effect;
            }

            effect.SetGeometry(cardSize, host.RenderSize);
            effect.Turn(tilt.AngleX, tilt.AngleY);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isSubscribedToRendering)
            {
                CompositionTarget.Rendering += OnRendering;
                _isSubscribedToRendering = true;
            }
        }

        // The rendering event is static, so a hand that left the screen must unsubscribe or it would never be released.
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
            _isSubscribedToRendering = false;
        }

        // WPF can raise Rendering more than once for the same frame, and only a new frame time advances the cards.
        // A long pause, such as a window being dragged, is capped so that the cards do not jump through many steps at once.
        private void OnRendering(object sender, EventArgs e)
        {
            TimeSpan renderingTime = ((RenderingEventArgs)e).RenderingTime;
            HandFan fan = CreateFan(GetSeatedItems().Count);
            if ((renderingTime == _lastRenderingTime) || (fan == null))
            {
                return;
            }

            _pendingSeconds = Math.Min(_pendingSeconds + (renderingTime - _lastRenderingTime).TotalSeconds, MaxCatchUpSeconds);
            _lastRenderingTime = renderingTime;
            UpdateHover();
            while (_pendingSeconds >= CardMotion.StepSeconds)
            {
                StepCards(fan);
                _pendingSeconds -= CardMotion.StepSeconds;
            }

            ApplyTransforms();
            AnnounceArrivals();
        }

        private void StepCards(HandFan fan)
        {
            List<object> seatedItems = GetSeatedItems();
            for (int index = 0; index < seatedItems.Count; index++)
            {
                object item = seatedItems[index];
                CardMotion motion = GetMotion(item, fan, index);
                motion.Target = GetTarget(item, fan, index);
                motion.TargetSlotAngle = motion.IsDragged ? 0 : fan.GetSlotAngle(index);
                motion.IsHovered = Equals(item, _hoveredItem);
                motion.Step();
            }

            foreach (KeyValuePair<object, Point> exit in _exitTargets)
            {
                StepLeavingCard(exit.Key, exit.Value);
            }
        }

        private void StepLeavingCard(object item, Point target)
        {
            CardMotion motion = _motions[item];
            motion.Target = target;
            motion.TargetSlotAngle = 0;
            motion.IsHovered = false;
            motion.Step();
            bool hasJustArrived = motion.HasArrived && !_departedItems.Contains(item);
            if (hasJustArrived)
            {
                _departedItems.Add(item);
                _arrivedItems.Add(item);
            }
        }

        // The listener removes the card from the items, which changes the collections this frame walks through.
        // The arrivals are therefore announced only after the frame is done with them.
        private void AnnounceArrivals()
        {
            List<object> arrivedItems = _arrivedItems.ToList();
            _arrivedItems.Clear();
            foreach (object arrivedItem in arrivedItems)
            {
                CardLeft?.Invoke(this, new CardEventArgs(arrivedItem));
            }
        }

        private List<object> GetSeatedItems()
        {
            return _order.Where(item => !_exitTargets.ContainsKey(item)).ToList();
        }

        private CardMotion GetMotion(object item, HandFan fan, int index)
        {
            CardMotion motion;
            if (!_motions.TryGetValue(item, out motion))
            {
                Point slot = fan.GetSlotCenter(index);
                motion = CreateMotion(new Point(slot.X, slot.Y + EntryDrop));
                _motions[item] = motion;
            }

            return motion;
        }

        private Point GetTarget(object item, HandFan fan, int index)
        {
            Point target = fan.GetSlotCenter(index);
            if (Equals(item, _draggedItem))
            {
                target = _pointer;
            }
            else if (Equals(item, SelectedItem))
            {
                target = new Point(target.X, target.Y - SelectedLift);
            }

            return target;
        }

        // The dragged card stays under the pointer, so it keeps the hover while it moves.
        private void UpdateHover()
        {
            object hoveredItem = null;
            if (_isPointerInside)
            {
                hoveredItem = _draggedItem ?? FindItemAt(_pointer);
            }

            bool hasHoverChanged = !Equals(hoveredItem, _hoveredItem);
            if (hasHoverChanged && (_hoveredItem != null) && _motions.TryGetValue(_hoveredItem, out CardMotion previousMotion))
            {
                previousMotion.Tilt.Release();
            }

            _hoveredItem = hoveredItem;
            bool canPickHoveredCard = (hoveredItem != null) && !IsReadOnly;
            Cursor = canPickHoveredCard ? Cursors.Hand : null;
            bool canTiltHoveredCard = (hoveredItem != null) && IsHoverTiltEnabled;
            if (canTiltHoveredCard && _motions.TryGetValue(hoveredItem, out CardMotion hoveredMotion))
            {
                hoveredMotion.Tilt.Follow(_pointer - hoveredMotion.Position, GetCardSize());
            }
        }

        private void ApplyTransforms()
        {
            Size cardSize = GetCardSize();
            for (int index = 0; index < _order.Count; index++)
            {
                object item = _order[index];
                Border host = GetHost(item);
                if ((host != null) && _motions.TryGetValue(item, out CardMotion motion))
                {
                    ((MatrixTransform)host.RenderTransform).Matrix = CreateMatrix(motion, host, cardSize);
                    bool isAboveHand = motion.IsDragged || motion.IsLeaving;
                    Panel.SetZIndex(host, isAboveHand ? DraggedLayer : index);
                    ShowFace(host, motion.IsShowingBack);
                    ApplyTilt(host, motion.Tilt, cardSize);
                }
            }
        }

        // The card is bent in its own space first, then scaled and turned about its center, and finally moved to its place.
        private Matrix CreateMatrix(CardMotion motion, Border host, Size cardSize)
        {
            Matrix matrix = Matrix.Identity;
            if (motion.IsHovered)
            {
                matrix.Translate(-TiltRoom, -TiltRoom);
                matrix.Append(HoverBulgeCommon.CreateMatrix(cardSize, _pointer - motion.Position));
                matrix.Translate(TiltRoom, TiltRoom);
            }

            matrix.Translate(-host.ActualWidth * Half, -host.ActualHeight * Half);
            matrix.Scale(motion.Scale * motion.FlipWidth, motion.Scale);
            matrix.Rotate(motion.AngleDegrees);
            matrix.Translate(motion.Position.X, motion.Position.Y);

            return matrix;
        }

        // The cards drawn later lie on top, so the search starts from the last one.
        private object FindItemAt(Point point)
        {
            object foundItem = null;
            List<object> seatedItems = GetSeatedItems();
            for (int index = seatedItems.Count - 1; index >= 0; index--)
            {
                object item = seatedItems[index];
                if ((foundItem == null) && IsOver(item, point))
                {
                    foundItem = item;
                }
            }

            return foundItem;
        }

        // The test undoes the transform of the card, so a turned or lifted card is hit where it is drawn.
        private bool IsOver(object item, Point point)
        {
            bool isOver = false;
            Border host = GetHost(item);
            if (host != null)
            {
                Matrix toHost = ((MatrixTransform)host.RenderTransform).Matrix;
                if (toHost.HasInverse)
                {
                    toHost.Invert();
                    Rect cardBounds = new Rect(new Point(TiltRoom, TiltRoom), GetCardSize());
                    isOver = cardBounds.Contains(toHost.Transform(point));
                }
            }

            return isOver;
        }

        private void StartDrag(int timestamp)
        {
            _draggedItem = _pressedItem;
            _pressedItem = null;
            _dragStartIndex = _order.IndexOf(_draggedItem);
            _lastDragPoint = _pointer;
            _lastDragTimestamp = timestamp;
            IsDragging = true;
            if (_motions.TryGetValue(_draggedItem, out CardMotion motion))
            {
                motion.IsDragged = true;
            }
        }

        // The other cards make room as soon as the dragged card passes over their place.
        private void TrackDrag(int timestamp)
        {
            int elapsedMilliseconds = timestamp - _lastDragTimestamp;
            if ((elapsedMilliseconds > 0) && _motions.TryGetValue(_draggedItem, out CardMotion motion))
            {
                motion.RegisterDragVelocity((_pointer.X - _lastDragPoint.X) * MillisecondsPerSecond / elapsedMilliseconds);
                _lastDragPoint = _pointer;
                _lastDragTimestamp = timestamp;
            }

            List<object> seatedItems = GetSeatedItems();
            HandFan fan = CreateFan(seatedItems.Count);
            if ((fan != null) && !IsDragOnly)
            {
                seatedItems.Remove(_draggedItem);
                seatedItems.Insert(fan.GetNearestIndex(_pointer.X), _draggedItem);
                _order.RemoveAll(item => seatedItems.Contains(item));
                _order.InsertRange(0, seatedItems);
            }
        }

        // The dropped card stays chosen, as it did in the ListBox.
        // The hand then takes the order of its items again, so a listener that does not move the card returns it to its place.
        private void FinishDrag()
        {
            object droppedItem = _draggedItem;
            _draggedItem = null;
            IsDragging = false;
            if (_motions.TryGetValue(droppedItem, out CardMotion motion))
            {
                motion.IsDragged = false;
            }

            // A card released over the drop target keeps its place in the hand until the listener decides to send it away.
            if (IsOverDropTarget())
            {
                SynchronizeOrder();
                CardDropped?.Invoke(this, new CardEventArgs(droppedItem));
                return;
            }

            if (IsDragOnly)
            {
                SynchronizeOrder();
                return;
            }

            SelectedItem = droppedItem;
            int newIndex = _order.IndexOf(droppedItem);
            if (newIndex != _dragStartIndex)
            {
                CardMoved?.Invoke(this, new CardMovedEventArgs(_dragStartIndex, newIndex));
            }

            SynchronizeOrder();
        }

        // A drag keeps the dragged card at the place the pointer chose even if the items change meanwhile.
        private void SynchronizeOrder()
        {
            int draggedIndex = _order.IndexOf(_draggedItem);
            _order.Clear();
            _order.AddRange(Items.Cast<object>());
            if ((draggedIndex >= 0) && _order.Remove(_draggedItem))
            {
                _order.Insert(Math.Min(draggedIndex, _order.Count), _draggedItem);
            }
            else
            {
                _draggedItem = null;
                IsDragging = false;
            }

            List<object> removedItems = _motions.Keys.Where(item => !_order.Contains(item)).ToList();
            foreach (object removedItem in removedItems)
            {
                _motions.Remove(removedItem);
                _exitTargets.Remove(removedItem);
                _departedItems.Remove(removedItem);
            }
        }

        private bool IsOverDropTarget()
        {
            bool isOverDropTarget = false;
            FrameworkElement dropTarget = DropTarget;
            if ((dropTarget != null) && dropTarget.IsVisible)
            {
                Point pointerOnTarget = TranslatePoint(_pointer, dropTarget);
                isOverDropTarget = new Rect(dropTarget.RenderSize).Contains(pointerOnTarget);
            }

            return isOverDropTarget;
        }

        private bool HasLeftClickArea(Point position)
        {
            Vector distance = position - _pressPoint;
            bool hasLeftClickArea = (Math.Abs(distance.X) > SystemParameters.MinimumHorizontalDragDistance)
                || (Math.Abs(distance.Y) > SystemParameters.MinimumVerticalDragDistance);

            return hasLeftClickArea;
        }

        private HandFan CreateFan(int cardCount)
        {
            HandFan fan = null;
            Size cardSize = GetCardSize();
            if ((cardSize.Width > 0) && (RenderSize.Width > 0))
            {
                fan = new HandFan(cardCount, cardSize, RenderSize) { IsFlipped = IsFanFlipped };
            }

            return fan;
        }

        private Size GetCardSize()
        {
            Size cardSize = new Size();
            Border host = null;
            if (_order.Count > 0)
            {
                host = GetHost(_order[0]);
            }

            if (host != null)
            {
                double room = TiltRoom * SidesPerAxis;
                cardSize = new Size(Math.Max(0, host.ActualWidth - room), Math.Max(0, host.ActualHeight - room));
            }

            return cardSize;
        }

        private Border GetHost(object item)
        {
            return ItemContainerGenerator.ContainerFromItem(item) as Border;
        }
    }
}
