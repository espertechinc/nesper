///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.lrreport
{
    public class Item
    {
        public Item(
            string assetId,
            Location location)
        {
            AssetId = assetId;
            Location = location;
        }

        public Item(
            string assetId,
            Location location,
            string type,
            string assetIdPassenger)
        {
            AssetId = assetId;
            Location = location;
            Type = type;
            AssetIdPassenger = assetIdPassenger;
        }

        public string Type { get; }

        public string AssetIdPassenger { get; }

        public string AssetId { get; }

        public Location Location { get; }
    }
} // end of namespace