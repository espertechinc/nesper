///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.lrreport
{
    public class ZoneFactory
    {
        public static IEnumerable<Zone> GetZones()
        {
            IList<Zone> zones = new List<Zone>();
            zones.Add(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
            return zones;
        }
    }
} // end of namespace