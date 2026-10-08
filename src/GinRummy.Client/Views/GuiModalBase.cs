using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    public class GuiModalBase : UserControl
    {
        private readonly LocalizationProvider _localization;
        private GuiWindowBase _host;
        private bool _isClosed;

        protected GuiModalBase()
        {
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            HasCloseButton = true;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Loaded += OnModalLoaded;
        }

        public event EventHandler Closed;

        public bool HasCloseButton { get; set; }

        internal GuiWindowBase Host
        {
            get { return _host; }
        }

        internal UIElement Entry { get; private set; }

        protected LocalizationProvider Localization
        {
            get { return _localization; }
        }

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

        protected virtual void RefreshFormattedText()
        {
        }

        protected void ShowModal(GuiModalBase modal)
        {
            _host.ShowModal(modal);
        }

        protected void NavigateTo(GuiModalBase nextScreen)
        {
            _host.ReplaceModal(this, nextScreen);
        }

        protected void EnterLobby(GuiWindowBase lobby)
        {
            _host.EnterLobby(lobby);
        }

        private void OnLocalizationChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshFormattedText();
        }

        // The focus moves to the first field so that a modal keeps the keyboard behavior of the windows it replaced.
        private void OnModalLoaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
    }
}
