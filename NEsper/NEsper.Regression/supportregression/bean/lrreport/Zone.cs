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
    public class Zone
    {
        public Zone(String name, Rectangle rectangle)
        {
            Name = name;
            Rectangle = rectangle;
        }

        public string Name { get; private set; }

        public Rectangle Rectangle { get; private set; }

        public static string[] GetZoneNames()
        {
            return new[] {"Z1", "Z2"};
        }
    }
}
