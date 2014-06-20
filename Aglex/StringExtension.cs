using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Aglex
{
    static class StringExtension
    {
        public static SizeF MeasureString(this string s, Font font, StringFormat strFormat)
        {
            SizeF result;
            using (var image = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(image))
                {
                    result = g.MeasureString(s, font, int.MaxValue, strFormat);
                }
            }

            return result;
        }
    }
}
