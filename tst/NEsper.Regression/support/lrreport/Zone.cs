///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.lrreport
{
    public class Zone
    {
        public Zone(
            string name,
            Rectangle rectangle)
        {
            Name = name;
            Rectangle = rectangle;
        }

        public string Name { get; }

        public Rectangle Rectangle { get; }

        public static string[] GetZoneNames()
        {
            return new[] {"Z1", "Z2"};
        }
    }
} // end of namespace