using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Controls
{
    // Draws the wordmark of the game the way the prototype does, with the target symbol between the two words of the brand.
    // The text is not written into the control, because it is read from the brand key of the dictionary.
    // The text is split on its blank so that the symbol can sit in the gap, which is what the prototype does with spacing.
    public partial class CtlBrandMark : UserControl
    {
        private const double DefaultMarkFontSize = 128.0;
        private const string BrandResourceKey = "Shared_AppTitle";
        private const double SymbolRatio = 0.898;
        private const double SymbolOverlap = -6.0;
        private const char WordSeparator = ' ';

        public static readonly DependencyProperty MarkFontSizeProperty =
            DependencyProperty.Register(
                "MarkFontSize",
                typeof(double),
                typeof(CtlBrandMark),
                new PropertyMetadata(DefaultMarkFontSize, OnAppearanceChanged));

        public static readonly DependencyProperty MarkForegroundProperty =
            DependencyProperty.Register(
                "MarkForeground",
                typeof(Brush),
                typeof(CtlBrandMark),
                new PropertyMetadata(Brushes.Gray, OnAppearanceChanged));

        private readonly LocalizationProvider _localization;

        public CtlBrandMark()
        {
            InitializeComponent();
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            Unloaded += OnControlUnloaded;
            Refresh();
        }

        public double MarkFontSize
        {
            get { return (double)GetValue(MarkFontSizeProperty); }
            set { SetValue(MarkFontSizeProperty, value); }
        }

        public Brush MarkForeground
        {
            get { return (Brush)GetValue(MarkForegroundProperty); }
            set { SetValue(MarkForegroundProperty, value); }
        }

        private static void OnAppearanceChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            CtlBrandMark mark = source as CtlBrandMark;
            if (mark != null)
            {
                mark.Refresh();
            }
        }

        private void Refresh()
        {
            if (lblBrandFirst == null)
            {
                return;
            }

            string brand = _localization.GetText(BrandResourceKey);
            string firstWord = brand;
            string secondWord = string.Empty;
            int separatorIndex = brand.IndexOf(WordSeparator);
            if (separatorIndex > 0)
            {
                firstWord = brand.Substring(0, separatorIndex);
                secondWord = brand.Substring(separatorIndex + 1).Trim();
            }

            lblBrandFirst.Text = firstWord;
            lblBrandSecond.Text = secondWord;
            lblBrandFirst.FontSize = MarkFontSize;
            lblBrandSecond.FontSize = MarkFontSize;
            lblBrandFirst.Foreground = MarkForeground;
            lblBrandSecond.Foreground = MarkForeground;
            double symbolSize = MarkFontSize * SymbolRatio;
            imgBrandMark.Width = symbolSize;
            imgBrandMark.Height = symbolSize;
            imgBrandMark.Margin = new Thickness(SymbolOverlap, 0, SymbolOverlap, 0);
        }

        private void OnLocalizationChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            _localization.PropertyChanged -= OnLocalizationChanged;
            Unloaded -= OnControlUnloaded;
        }
    }
}
