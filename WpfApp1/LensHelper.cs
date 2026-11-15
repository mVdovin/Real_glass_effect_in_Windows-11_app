using System;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public static class LensHelper
    {
        // 1. Активация линзы
        public static readonly DependencyProperty IsLensProperty =
            DependencyProperty.RegisterAttached("IsLens", typeof(bool), typeof(LensHelper),
                new PropertyMetadata(false, OnIsLensChanged));

        public static bool GetIsLens(DependencyObject obj) => (bool)obj.GetValue(IsLensProperty);
        public static void SetIsLens(DependencyObject obj, bool value) => obj.SetValue(IsLensProperty, value);

        // 2. Целевой контент (ContentArea)
        public static readonly DependencyProperty TargetContentProperty =
            DependencyProperty.RegisterAttached("TargetContent", typeof(Visual), typeof(LensHelper),
                new PropertyMetadata(null));

        public static Visual GetTargetContent(DependencyObject obj) => (Visual)obj.GetValue(TargetContentProperty);
        public static void SetTargetContent(DependencyObject obj, Visual value) => obj.SetValue(TargetContentProperty, value);

        // 3. Радиус скругления
        public static readonly DependencyProperty LensCornerRadiusProperty =
            DependencyProperty.RegisterAttached("LensCornerRadius", typeof(double), typeof(LensHelper),
                new PropertyMetadata(0.0));

        public static double GetLensCornerRadius(DependencyObject obj) => (double)obj.GetValue(LensCornerRadiusProperty);
        public static void SetLensCornerRadius(DependencyObject obj, double value) => obj.SetValue(LensCornerRadiusProperty, value);

        // 4. Смещение по вертикали (те самые +32 для калибровки)
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(LensHelper),
                new PropertyMetadata(32.0));

        public static double GetVerticalOffset(DependencyObject obj) => (double)obj.GetValue(VerticalOffsetProperty);
        public static void SetVerticalOffset(DependencyObject obj, double value) => obj.SetValue(VerticalOffsetProperty, value);

        private static void OnIsLensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                {
                    // Подписываемся на обновление каждого кадра для плавности
                    CompositionTarget.Rendering += (s, args) => UpdateElementLens(element);
                }
            }
        }

        private static void UpdateElementLens(FrameworkElement lensElement)
        {
            // Пытаемся найти VisualBrush в Background
            VisualBrush? brush = null;
            if (lensElement is System.Windows.Controls.Border b && b.Background is VisualBrush vb) brush = vb;
            else if (lensElement is System.Windows.Controls.Control c && c.Background is VisualBrush vb2) brush = vb2;

            var contentArea = GetTargetContent(lensElement);

            // Проверки безопасности
            if (brush == null || contentArea == null || !lensElement.IsLoaded || !lensElement.IsVisible)
                return;

            try
            {
                // --- Логика позиционирования ---
                double scale = 1.0;
                double vOffset = GetVerticalOffset(lensElement);

                // Точка центра текущего элемента
                System.Windows.Point lensCenter = new System.Windows.Point(lensElement.ActualWidth / 2, lensElement.ActualHeight / 2);

                // Перевод в экранные координаты, затем в координаты контента
                System.Windows.Point screenPoint = lensElement.PointToScreen(lensCenter);
                System.Windows.Point contentPoint = ((Visual)contentArea).PointFromScreen(screenPoint);

                // Расчет окна захвата (Viewbox)
                double vbWidth = lensElement.ActualWidth * scale;
                double vbHeight = lensElement.ActualHeight * scale;

                double vbX = contentPoint.X - (vbWidth / 2);
                double vbY = contentPoint.Y - (vbHeight / 2) + vOffset;

                // Применяем результат к кисти
                brush.Viewbox = new Rect(vbX, vbY, vbWidth, vbHeight);

                // --- Логика обрезки (Clip) ---
                double radius = GetLensCornerRadius(lensElement);

                // Создаем или обновляем геометрию обрезки
                if (!(lensElement.Clip is RectangleGeometry rectGeom))
                {
                    rectGeom = new RectangleGeometry();
                    lensElement.Clip = rectGeom;
                }

                rectGeom.Rect = new Rect(0, 0, lensElement.ActualWidth, lensElement.ActualHeight);
                rectGeom.RadiusX = radius;
                rectGeom.RadiusY = radius;
            }
            catch (InvalidOperationException)
            {
                // Исключение может возникнуть, если визуальное дерево еще не готово
            }
        }
    }
}
