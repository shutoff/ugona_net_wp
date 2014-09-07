using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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

        public AddressBox()
        {
            this.TextAlignment = TextAlignment.Center;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        static Brush blue = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xb5, 0xe5));

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
                span.Foreground = blue;
            }
            richTextBox.Blocks.Clear();
            richTextBox.Blocks.Add(paragraph);
        }
    }
}
