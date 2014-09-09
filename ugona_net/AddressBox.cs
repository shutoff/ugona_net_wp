using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ugona_net
{
    public sealed class AddressBox : RichTextBox
    {
        public const string TextPropertyName = "Text";

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(TextPropertyName,
                                        typeof(string),
                                        typeof(RichTextBox),
                                        new PropertyMetadata(
                                            new PropertyChangedCallback
                                                (TextPropertyChanged)));

        public event RoutedEventHandler Click;

        public AddressBox()
        {
            this.TextAlignment = TextAlignment.Center;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        DateTime downTime;
        Point downPoint;

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            downTime = DateTime.Now;
            downPoint = e.GetPosition(this);
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            DateTime upTime = DateTime.Now;
            Point upPoint = e.GetPosition(this);
            long delta = (upTime.Ticks - downTime.Ticks) / 10000;
            double dx = upPoint.X - downPoint.X;
            double dy = upPoint.Y - downPoint.Y;
            double distance = dx * dx + dy * dy;
            if ((delta > 900) || (distance > 400))
                return;
            if (Click != null)
                Click(this, e);
        }

        private static void TextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var richTextBox = (AddressBox)dependencyObject;
            var text = (string)dependencyPropertyChangedEventArgs.NewValue;

            char[] separators = { '|' };
            String[] parts = text.Split(separators);
            Paragraph paragraph = new Paragraph();
            bool odd = false;
            foreach (String part in parts)
            {
                Span span = new Span();
                span.Inlines.Add(part);
                paragraph.Inlines.Add(span);
                if (odd)
                {
                    odd = false;
                    continue;
                }
                odd = true;
                span.FontWeight = FontWeights.Bold;
                span.Foreground = Colors.BlueBrush;
            }
            richTextBox.Blocks.Clear();
            richTextBox.Blocks.Add(paragraph);
        }
    }
}
