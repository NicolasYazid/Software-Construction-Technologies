using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Base of every modal: the smaller screens that open inside a main screen instead of in
    /// a window of their own. The main screen dims itself behind the modal, and the modal keeps
    /// itself subscribed to the culture change while it is open.
    /// </summary>
    public class GuiModalBase : UserControl
    {
        private readonly LocalizationProvider _localization;
        private GuiWindowBase _host;
        private bool _isClosed;

        /// <summary>
        /// Subscribes the modal to the culture change and centres it on the screen that hosts
        /// it, unless the modal places itself otherwise.
        /// </summary>
        protected GuiModalBase()
        {
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            HasCloseButton = true;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Loaded += OnModalLoaded;
        }

        /// <summary>
        /// Raised once the modal closes, whatever closed it.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Gets or sets whether the screen that hosts the modal adds a closing cross to its
        /// corner. The modals that already have a way to close leave it off.
        /// </summary>
        public bool HasCloseButton { get; set; }

        internal GuiWindowBase Host
        {
            get { return _host; }
        }

        // The layer of the host that holds the modal and the shade behind it.
        internal UIElement Entry { get; private set; }

        /// <summary>
        /// Gets the localization provider shared by the whole application.
        /// </summary>
        protected LocalizationProvider Localization
        {
            get { return _localization; }
        }

        /// <summary>
        /// Closes the modal, takes it off the screen that hosts it and raises
        /// <see cref="Closed"/>.
        /// </summary>
        public void Close()
        {
            if (_isClosed)
            {
                return;
            }

            _isClosed = true;
            _localization.PropertyChanged -= OnLocalizationChanged;
            Loaded -= OnModalLoaded;
            if (_host != null)
            {
                _host.RemoveModal(this);
            }

            EventHandler closed = Closed;
            if (closed != null)
            {
                closed(this, EventArgs.Empty);
            }
        }

        internal void AttachTo(GuiWindowBase host, UIElement entry)
        {
            _host = host;
            Entry = entry;
        }

        /// <summary>
        /// Rebuilds the texts that are assembled from a format string. Modals that show
        /// counters, durations or interpolated values override it.
        /// </summary>
        protected virtual void RefreshFormattedText()
        {
        }

        /// <summary>
        /// Opens another modal over this one, on the same main screen.
        /// </summary>
        /// <param name="modal">Modal that opens over this one.</param>
        protected void ShowModal(GuiModalBase modal)
        {
            _host.ShowModal(modal);
        }

        /// <summary>
        /// Opens the next modal of the flow in the place of this one.
        /// </summary>
        /// <param name="nextScreen">Modal that takes the place of this one.</param>
        protected void NavigateTo(GuiModalBase nextScreen)
        {
            _host.ReplaceModal(this, nextScreen);
        }

        /// <summary>
        /// Opens a lobby in place of the main menu that hosts this modal.
        /// </summary>
        /// <param name="lobby">Lobby the player enters.</param>
        protected void EnterLobby(GuiWindowBase lobby)
        {
            _host.EnterLobby(lobby);
        }

        private void OnLocalizationChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshFormattedText();
        }

        // The first field of the modal takes the keyboard, as the first field of a window did.
        private void OnModalLoaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
    }
}
