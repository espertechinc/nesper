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
    public class Item
    {
        public Item(String assetId, Location location)
        {
            AssetId = assetId;
            Location = location;
        }

        public Item(String assetId, Location location, String type, String assetIdPassenger)
        {
            AssetId = assetId;
            Location = location;
            Type = type;
            AssetIdPassenger = assetIdPassenger;
        }

        public string Type { get; private set; }

        public string AssetIdPassenger { get; private set; }

        public string AssetId { get; private set; }

        public Location Location { get; private set; }
    }
}
