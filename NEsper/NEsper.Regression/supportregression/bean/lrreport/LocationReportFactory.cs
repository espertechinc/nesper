///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.supportregression.bean.lrreport
{
    public class LocationReportFactory
    {
        public static LocationReport MakeSmall()
        {
            var items = new List<Item>();
            items.Add(new Item("P00002", new Location(40, 40), "P", null));
            items.Add(new Item("L00001", new Location(42, 41), "L", "P00002"));    // This is luggage #L00001 beloning to P00002
            items.Add(new Item("P00020", new Location(0, 0), "P", null));
            return new LocationReport(items);
        }

        public static LocationReport MakeLarge()
        {
            var items = new List<Item>();
            items.Add(new Item("P00002", new Location(40, 40), "P", null));
            items.Add(new Item("L00001", new Location(42, 41), "L", "P00002"));    // This is luggage #L00001 beloning to P00002
            items.Add(new Item("L00002", new Location(43, 43), "L", "P00002"));
            items.Add(new Item("P00001", new Location(10, 10), "P", null));
            items.Add(new Item("L00000", new Location(99, 97), "L", "P00001"));
            items.Add(new Item("P00004", new Location(20, 20), "P", null));
            items.Add(new Item("P00002", new Location(40, 40), "P", null));
            items.Add(new Item("L00003", new Location(29, 26), "L", "P00004"));
            items.Add(new Item("E00011", new Location(90, 95), "P", null));
            items.Add(new Item("A00010", new Location(104, 101), "L", "E00011"));
            items.Add(new Item("A00011", new Location(96, 100), "L", "E00011"));
            items.Add(new Item("E00010", new Location(90, 95), "P", null));
            items.Add(new Item("L00009", new Location(102, 101), "L", "E00010"));
            items.Add(new Item("P00005", new Location(30, 30), "P", null));
            items.Add(new Item("L00004", new Location(26, 27), "L", "P00005"));
            items.Add(new Item("L00005", new Location(30, 28), "L", "P00005"));
            items.Add(new Item("P00007", new Location(90, 95), "P", null));
            items.Add(new Item("L00006", new Location(96, 100), "L", "P00007"));
            items.Add(new Item("P00008", new Location(100, 100), "P", null));
            items.Add(new Item("L00007", new Location(10, 12), "L", "P00008"));
            items.Add(new Item("L00008", new Location(10, 12), "L", "P00008"));
            return new LocationReport(items);
        }

        /// <summary>Return all luggages separated from the owner. </summary>
        /// <param name="lr">event</param>
        /// <returns>list</returns>
        public static IList<Item> FindSeparatedLuggage(LocationReport lr)
        {
            // loop over all luggages
            // find the location of the owner of the luggage
            // Compute distance luggage to owner
            // if distance > 10 add original-owner

            var result = new List<Item>();
            foreach (Item item in lr.Items)
            {
                if (item.Type.Equals("L"))
                {
                    String belongTo = item.AssetIdPassenger;

                    Item owner = null;
                    foreach (Item ownerItem in lr.Items)
                    {
                        if (ownerItem.Type.Equals("P"))
                        {
                            if (ownerItem.AssetId.Equals(belongTo))
                            {
                                owner = ownerItem;
                            }
                        }
                    }

                    if (owner == null)
                    {
                        continue;
                    }

                    double distanceOwner = LRUtil.Distance(owner.Location.X, owner.Location.Y,
                            item.Location.X, item.Location.Y);
                    if (distanceOwner > 20)
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        public static Item FindPotentialNewOwner(LocationReport lr, Item luggageItem)
        {

            // for a given luggage find the owner that is nearest to it
            Item passenger = null;
            double distanceMin = Int32.MaxValue;
            foreach (Item item in lr.Items)
            {
                if (item.Type.Equals("P"))
                {
                    String who = item.AssetId;
                    if (luggageItem.AssetIdPassenger.Equals(who))
                    {
                        continue;
                    }

                    double distance = LRUtil.Distance(luggageItem.Location.X, luggageItem.Location.Y,
                            item.Location.X, item.Location.Y);

                    if (passenger == null || distance < distanceMin)
                    {
                        passenger = item;
                        distanceMin = distance;
                    }
                }
            }
            return passenger;
        }
    }
}
