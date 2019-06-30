using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.common.@internal.supportunit.geom
{
    public class Rectangle2D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Width { get; set; }
        public T Height { get; set; }

        public Rectangle2D()
        {
        }

        public Rectangle2D(T x,
            T y)
        {
            X = x;
            Y = y;
        }

        public Rectangle2D(T x,
            T y,
            T width,
            T height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
