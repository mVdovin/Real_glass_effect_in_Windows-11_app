using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();


            // Объединяем вызовы в один блок
            Loaded += (s, e) =>
            {
                NavigateTo("Чати"); // Загружаем контент по умолчанию

                _targetOffset = MainScroll.VerticalOffset;
            };
        }
        private bool _isDragging;
        private System.Windows.Point _clickPosition;
        // Переменные для фиксации предыдущей позиции, чтобы понять направление движения
        private double _lastX;
        private double _lastY;
        //=============================================>
        private void DraggableBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                _isDragging = true;
                _clickPosition = e.GetPosition(border);

                // Запоминаем стартовые координаты мыши на экране
                System.Windows.Point startPos = e.GetPosition(MainCanvas);
                _lastX = startPos.X;
                _lastY = startPos.Y;

                border.CaptureMouse();

                // Останавливаем старые анимации возврата формы, если они работали
                //JellySkew.BeginAnimation(SkewTransform.AngleXProperty, null);
                //JellySkew.BeginAnimation(SkewTransform.AngleYProperty, null);
            }
        }
        // 2. Перемещение мыши
        private void DraggableBorder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging && sender is Border border)
            {
                System.Windows.Point currentPos = e.GetPosition(MainCanvas);

                // 1. Стандартное перемещение по Canvas
                double newLeft = currentPos.X - _clickPosition.X;
                double newTop = currentPos.Y - _clickPosition.Y;

                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + border.ActualWidth > MainCanvas.ActualWidth) newLeft = MainCanvas.ActualWidth - border.ActualWidth;
                if (newTop + border.ActualHeight > MainCanvas.ActualHeight) newTop = MainCanvas.ActualHeight - border.ActualHeight;

                Canvas.SetLeft(border, newLeft);
                Canvas.SetTop(border, newTop);

                // 2. РАСЧЕТ ЖЕЛИРОВАНИЯ (Деформация во время движения)
                // Вычисляем смещение (скорость) по осям X и Y
                double deltaX = currentPos.X - _lastX;
                double deltaY = currentPos.Y - _lastY;

                // Задаем угол наклона пропорционально скорости движения. 
                // Ограничиваем жестко рамками от -20 до 20 градусов, чтобы не скрутило в узел
                double targetAngleX = Math.Max(-20, Math.Min(20, deltaX * 0.8));
                double targetAngleY = Math.Max(-20, Math.Min(20, deltaY * 0.8));

                // Применяем наклон прямо сейчас
                //JellySkew.AngleX = targetAngleX;
                //JellySkew.AngleY = targetAngleY;

                // Сохраняем текущие координаты для следующего кадра
                _lastX = currentPos.X;
                _lastY = currentPos.Y;
            }
        }
        //=============================================>
        // 3. Отпускание левой кнопки мыши
        private void DraggableBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && sender is Border border)
            {
                _isDragging = false;
                border.ReleaseMouseCapture();

                // 3. ПРУЖИННЫЙ ОТСКОК (Когда отпустили мышь)
                // Создаем функцию плавности «Эластичная пружина»
                ElasticEase elasticBehavior = new ElasticEase
                {
                    Oscillations = 3, // Сколько раз забордерится туда-сюда перед остановкой
                    Springiness = 1,   // Жесткость пружины (чем меньше, тем мягче желе)
                    EasingMode = EasingMode.EaseOut
                };

                // Анимация возврата угла X в 0
                DoubleAnimation returnX = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = elasticBehavior
                };

                // Анимация возврата угла Y в 0
                DoubleAnimation returnY = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = elasticBehavior
                };

                // Запускаем пружины
                //JellySkew.BeginAnimation(SkewTransform.AngleXProperty, returnX);
                //JellySkew.BeginAnimation(SkewTransform.AngleYProperty, returnY);
            }
        }
        //=============================================>
        private void Block1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Вызываем наш метод плавной анимации меню
            ToggleMenu();

            // Помечаем событие как обработанное, чтобы оно не шло дальше к родителю
            e.Handled = true;
        }
        private void ToggleMenu()
        {
            if (mainMenu == null) return;

            Storyboard storyboard = new Storyboard();
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.25); // Длительность анимации (четверть секунды)

            // Если меню скрыто — плавно показываем
            if (mainMenu.Visibility == Visibility.Collapsed)
            {
                mainMenu.Visibility = Visibility.Visible; // Делаем доступным для рендеринга
                opacityAnimation.From = 0.0;
                opacityAnimation.To = 1.0;
            }
            // Если меню открыто — плавно скрываем
            else
            {
                opacityAnimation.From = 1.0;
                opacityAnimation.To = 0.0;

                // По окончании анимации полностью убираем элемент из разметки
                opacityAnimation.Completed += (s, e) => mainMenu.Visibility = Visibility.Collapsed;
            }

            // Привязываем анимацию к свойству Opacity нашего mainMenu
            Storyboard.SetTarget(opacityAnimation, mainMenu);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Border.OpacityProperty));

            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin();
        }
        //=============================================>








        //=============================================>
        private string CurrentTheme = "Dark";
        //=============================================>переход по кнопкам с генерацией контента для примера
        private void NavigateTo(string? pageName)
        {
            var items = new List<object>();
            var rnd = new Random();
            System.Windows.Media.Brush[] colors = { System.Windows.Media.Brushes.DodgerBlue, System.Windows.Media.Brushes.Crimson, System.Windows.Media.Brushes.LimeGreen, System.Windows.Media.Brushes.Orange, System.Windows.Media.Brushes.MediumPurple };
            for (int i = 0; i < 30; i++)
                items.Add(new { Name = $"{pageName} #{i + 1}", LastMsg = "Тут буде ваш текст під склом! І ще трохи тексту тут! Тут буде ваш текст під склом! І ще трохи тексту тут!", AvatarBrush = colors[rnd.Next(colors.Length)] });

            System.Windows.Controls.ListBox list = new System.Windows.Controls.ListBox
            {
                ItemsSource = items,
                ItemTemplate = (DataTemplate)this.Resources["ChatItemTemplate"],
                ItemContainerStyle = (Style)this.Resources["ChatItemStyle"]
            };

            RootFrame.Navigate(new Page { Content = list });
        }
        //=============================================>
        public void SwitchTheme(string themeName)
        {
            // 1. Формируем путь к новому файлу
            var uri = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);
            ResourceDictionary? resourceDict = System.Windows.Application.LoadComponent(uri) as ResourceDictionary;

            if (resourceDict != null)
            {
                // 2. Очищаем старые кастомные темы (если их было несколько)
                // ВАЖНО: Мы ищем именно наш файл по части пути, чтобы не удалить словари иконок
                var mergedDicts = System.Windows.Application.Current.Resources.MergedDictionaries;

                for (int i = 0; i < mergedDicts.Count; i++)
                {
                    if (mergedDicts[i].Source != null && mergedDicts[i].Source.OriginalString.Contains("Themes/"))
                    {
                        mergedDicts.RemoveAt(i);
                        i--; // Сдвигаем индекс после удаления
                    }
                }

                // 3. Добавляем новый словарь
                mergedDicts.Add(resourceDict);
            }
        }
        //=============================================>плвный скролл с накапливанием дельты и плавной анимацией
        private double _targetOffset = 0;
        private void MainScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            // Накапливаем смещение: добавляем дельту к финальной цели, а не к текущей позиции
            // Это убирает рывки при быстром вращении колеса
            _targetOffset -= e.Delta;

            // Ограничиваем цель границами контента
            if (_targetOffset < 0) _targetOffset = 0;
            if (_targetOffset > MainScroll.ScrollableHeight) _targetOffset = MainScroll.ScrollableHeight;

            // Анимация
            DoubleAnimation anim = new DoubleAnimation
            {
                To = _targetOffset,
                Duration = TimeSpan.FromMilliseconds(600), // Оптимальное время для мягкости
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            // Запускаем анимацию на нашем Helper-свойстве
            MainScroll.BeginAnimation(ScrollHelper.ScrollOffsetProperty, anim);
        }
        //=============================================>
        private void Header_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }
        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => System.Windows.Application.Current.Shutdown();
        //=============================================>
        private void ThemeSwither(object sender, RoutedEventArgs e)
        {
            if (CurrentTheme == "Light")
            {
                CurrentTheme = "Dark";
            }
            else
            {
                CurrentTheme = "Light";
            }
            SwitchTheme(CurrentTheme);
        }
    }






    public class NegativeValueConverter : IValueConverter
    {
        // Этот метод срабатывает, когда значение идет ИЗ слайдера В Margin
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // Умножаем значение на -1, чтобы сделать его отрицательным
                double negativeValue = doubleValue * -1;

                // Возвращаем объект Thickness (так как Margin принимает именно его).
                // В данном случае мы делаем отступ слева (Left), остальные стороны — 0.
                // Если тебе нужен отступ со всех сторон, напиши: new Thickness(negativeValue)
                return new Thickness(negativeValue);
            }

            return new Thickness(0);
        }

        // Этот метод нужен для обратной связи, но для Margin он не используется
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class SizeToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что к нам пришли два валидных числа (ширина и высота)
            if (values.Length >= 2 && values[0] is double width && values[1] is double height)
            {
                // Создаем Rect, где X=0, Y=0, а ширина и высота берутся от Бордера
                return new Rect(0, 0, width, height);
            }

            return new Rect(0, 0, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}