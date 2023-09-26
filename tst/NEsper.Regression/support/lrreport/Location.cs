///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.lrreport
{
    [Serializable]
    public class Location
    {
        public Location(
            int x,
            int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public override string ToString()
        {
            return "Location{" +
                   "x=" +
                   X +
                   ", y=" +
                   Y +
                   '}';
        }
    }
} // end of namespace