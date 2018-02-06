///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean.lrreport
{
    public class LRUtil
    {
        public static double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }

        public static bool Inrect(Rectangle r, Location l)
        {
            int minX = GetMin(r.X1, r.X2);
            int minY = GetMin(r.Y1, r.Y2);
            int maxX = GetMax(r.X1, r.X2);
            int maxY = GetMax(r.Y1, r.Y2);
            if (l.X >= minX &&
                    l.X <= maxX &&
                    l.Y >= minY &&
                    l.Y <= maxY)
            {
                return true;
            }
            return false;
        }

        private static int GetMin(int numOne, int numTwo)
        {
            return numOne < numTwo ? numOne : numTwo;
        }

        private static int GetMax(int numOne, int numTwo)
        {
            return numOne > numTwo ? numOne : numTwo;
        }
    }
}
