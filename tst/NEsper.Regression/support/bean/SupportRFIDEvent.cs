///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportRFIDEvent
    {
        public SupportRFIDEvent(
            string mac,
            string zoneID) : this(null, mac, zoneID)
        {
        }

        public SupportRFIDEvent(
            string locationReportId,
            string mac,
            string zoneID)
        {
            LocationReportId = locationReportId;
            Mac = mac;
            ZoneID = zoneID;
        }

        public string LocationReportId { get; }

        public string Mac { get; }

        public string ZoneID { get; }
    }
} // end of namespace