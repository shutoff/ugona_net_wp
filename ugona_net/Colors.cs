using System.Windows.Media;

namespace ugona_net
{
    class Colors
    {
        static private Brush error_brush;

        static public Brush ErrorBrush
        {
            get
            {
                if (error_brush == null)
                {
                    error_brush = new SolidColorBrush(Color.FromArgb(255, 255, 64, 64));
                }
                return error_brush;
            }
        }

        static private Brush blue_brush;

        static public Brush BlueBrush
        {
            get
            {
                if (blue_brush == null)
                {
                    blue_brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xb5, 0xe5));
                }
                return blue_brush;
            }
        }       
    }
}
