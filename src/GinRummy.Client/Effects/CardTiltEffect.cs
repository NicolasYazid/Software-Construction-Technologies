using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace GinRummy.Client.Effects
{
    public class CardTiltEffect : ShaderEffect
    {
        private const double FieldOfViewDegrees = 90.0;
        private const double Half = 0.5;
        private const double DegreesPerHalfTurn = 180.0;
        private const string ShaderUri = "pack://application:,,,/GinRummy.Client;component/Effects/CardTilt.ps";

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(CardTiltEffect), 0, SamplingMode.NearestNeighbor);

        private static readonly DependencyProperty _rowXProperty = RegisterConstant("RowX", 0);
        private static readonly DependencyProperty _rowYProperty = RegisterConstant("RowY", 1);
        private static readonly DependencyProperty _rowZProperty = RegisterConstant("RowZ", 2);
        private static readonly DependencyProperty _hostScaleProperty = RegisterConstant("HostScale", 3);
        private static readonly PixelShader _shader = CreateShader();

        public CardTiltEffect()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
            Turn(0, 0);
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public void SetGeometry(Size cardSize, Size hostSize)
        {
            double growth = 1 + GetHalfFieldTangent();
            double horizontalRatio = hostSize.Width / cardSize.Width;
            double verticalRatio = hostSize.Height / cardSize.Height;
            SetValue(_hostScaleProperty, new Point4D(
                horizontalRatio / growth,
                verticalRatio / growth,
                1 / horizontalRatio,
                1 / verticalRatio));
        }

        public void Turn(double angleXDegrees, double angleYDegrees)
        {
            double sinX = Math.Sin(ToRadians(angleXDegrees));
            double cosX = Math.Cos(ToRadians(angleXDegrees));
            double sinY = Math.Sin(ToRadians(angleYDegrees));
            double cosY = Math.Cos(ToRadians(angleYDegrees));
            double cameraDistance = Half / GetHalfFieldTangent();
            double cardDistance = cameraDistance + Half;
            SetValue(_rowXProperty, new Point4D(cosY, sinY * sinX, sinY * cosX, cameraDistance));
            SetValue(_rowYProperty, new Point4D(0, cosX, -sinX, cardDistance));
            SetValue(_rowZProperty, new Point4D(-sinY, cosY * sinX, cosY * cosX, 0));
        }

        private static DependencyProperty RegisterConstant(string name, int register)
        {
            return DependencyProperty.Register(
                name,
                typeof(Point4D),
                typeof(CardTiltEffect),
                new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(register)));
        }

        private static PixelShader CreateShader()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = new Uri(ShaderUri, UriKind.Absolute);
            shader.Freeze();

            return shader;
        }

        private static double GetHalfFieldTangent()
        {
            return Math.Tan(ToRadians(FieldOfViewDegrees) * Half);
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / DegreesPerHalfTurn;
        }
    }
}
