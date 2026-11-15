using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace WpfApp1
{
    public class LensEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new PixelShader()
        {
            UriSource = new Uri("pack://application:,,,/WpfApp1;component/GlassEffect.ps")
        };

        public LensEffect()
        {
            this.PixelShader = _pixelShader;
            foreach (var prop in new[] { InputProperty, EdgeWidthProperty, DistortionProperty,
                ReflectPowerProperty, RGBShiftProperty, ObjWidthProperty, ObjHeightProperty, ShapeTypeProperty })
            {
                UpdateShaderValue(prop);
            }
        }
        public static readonly DependencyProperty EdgeWidthProperty = DependencyProperty.Register("EdgeWidth", typeof(double), typeof(LensEffect), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(0), CoerceEdgeWidth));

        // Коэрция: не дает значению опуститься ниже 0.001
        private static object CoerceEdgeWidth(DependencyObject d, object baseValue)
        {
            double val = (double)baseValue;
            return val < 0.001 ? 0.001 : val;
        }
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(LensEffect), 0);
        public System.Windows.Media.Brush Input { get => (System.Windows.Media.Brush)GetValue(InputProperty); set => SetValue(InputProperty, value); }

        public double EdgeWidth { get => (double)GetValue(EdgeWidthProperty); set => SetValue(EdgeWidthProperty, value); }

        public static readonly DependencyProperty DistortionProperty = DependencyProperty.Register("Distortion", typeof(double), typeof(LensEffect), new UIPropertyMetadata(0.4, PixelShaderConstantCallback(1)));
        public double Distortion { get => (double)GetValue(DistortionProperty); set => SetValue(DistortionProperty, value); }

        public static readonly DependencyProperty ReflectPowerProperty = DependencyProperty.Register("ReflectPower", typeof(double), typeof(LensEffect), new UIPropertyMetadata(0.3, PixelShaderConstantCallback(2)));
        public double ReflectPower { get => (double)GetValue(ReflectPowerProperty); set => SetValue(ReflectPowerProperty, value); }

        public static readonly DependencyProperty RGBShiftProperty = DependencyProperty.Register("RGBShift", typeof(double), typeof(LensEffect), new UIPropertyMetadata(0.01, PixelShaderConstantCallback(3)));
        public double RGBShift { get => (double)GetValue(RGBShiftProperty); set => SetValue(RGBShiftProperty, value); }

        public static readonly DependencyProperty ObjWidthProperty = DependencyProperty.Register("ObjWidth", typeof(double), typeof(LensEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(4)));
        public double ObjWidth { get => (double)GetValue(ObjWidthProperty); set => SetValue(ObjWidthProperty, value); }

        public static readonly DependencyProperty ObjHeightProperty = DependencyProperty.Register("ObjHeight", typeof(double), typeof(LensEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(5)));
        public double ObjHeight { get => (double)GetValue(ObjHeightProperty); set => SetValue(ObjHeightProperty, value); }

        public static readonly DependencyProperty ShapeTypeProperty = DependencyProperty.Register("ShapeType", typeof(double), typeof(LensEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(6)));
        public double ShapeType { get => (double)GetValue(ShapeTypeProperty); set => SetValue(ShapeTypeProperty, value); }
    }
}