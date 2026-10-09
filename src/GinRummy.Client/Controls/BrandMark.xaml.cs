using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Controls
{
    // The brand is split on its blank so that the target symbol sits between its two words, as the prototype draws the wordmark.
    public partial class BrandMark : UserControl
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
                typeof(BrandMark),
                new PropertyMetadata(DefaultMarkFontSize, OnAppearanceChanged));

        public static readonly DependencyProperty MarkForegroundProperty =
            DependencyProperty.Register(
                "MarkForeground",
                typeof(Brush),
                typeof(BrandMark),
                new PropertyMetadata(Brushes.Gray, OnAppearanceChanged));

        private readonly LocalizationProvider _localization;

        public BrandMark()
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
            BrandMark mark = source as BrandMark;
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
