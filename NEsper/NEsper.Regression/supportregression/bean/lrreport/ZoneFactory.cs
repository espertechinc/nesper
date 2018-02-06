///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportregression.bean.lrreport
{
    public class ZoneFactory
    {
        public static IEnumerable<Zone> GetZones() {
            var zones = new List<Zone>();
            zones.Add(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
            return zones;
        }
    }
}
