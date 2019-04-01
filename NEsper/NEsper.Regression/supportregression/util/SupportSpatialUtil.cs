///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regression.spatial;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportSpatialUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void AssertRectanglesSingleValue(EPServiceProvider epService, SupportUpdateListener listener, IList<BoundingBox> rectangles, params string[] matches) {
            for (var i = 0; i < rectangles.Count; i++) {
                BoundingBox box = rectangles[i];
                SendRectangle(epService, "R" + box.ToString(), box.MinX, box.MinY, box.MaxX - box.MinX, box.MaxY - box.MinY);
                var c0 = listener.AssertOneGetNewAndReset().Get("c0").ToString();
                Assert.AreEqual(matches[i], c0, "for box " + i);
            }
        }
    
        public static void AssertRectanglesManyRow(EPServiceProvider epService, SupportUpdateListener listener, IList<BoundingBox> rectangles, params string[] matches) {
            for (var i = 0; i < rectangles.Count; i++) {
                BoundingBox box = rectangles[i];
                SendRectangle(epService, "R" + box.ToString(), box.MinX, box.MinY, box.MaxX - box.MinX, box.MaxY - box.MinY);
                if (matches[i] == null) {
                    if (listener.IsInvoked) {
                        Assert.Fail("Unexpected output for box " + i + ": " + SortJoinProperty(listener.GetAndResetLastNewData(), "c0"));
                    }
                } else {
                    if (!listener.IsInvoked) {
                        Assert.Fail("No output for box " + i);
                    }
                    Assert.AreEqual(matches[i], SortJoinProperty(listener.GetAndResetLastNewData(), "c0"));
                }
            }
        }
    
        public static void SendPoint(EPServiceProvider epService, string id, double x, double y) {
            epService.EPRuntime.SendEvent(new SupportSpatialPoint(id, x, y));
        }
    
        public static void SendPoint(EPServiceProvider epService, string id, double x, double y, string category) {
            epService.EPRuntime.SendEvent(new SupportSpatialPoint(id, x, y, category));
        }
    
        public static void SendRectangle(EPServiceProvider epService, string id, double x, double y, double width, double height) {
            epService.EPRuntime.SendEvent(new SupportSpatialAABB(id, x, y, width, height));
        }
    
        public static void SendAssert(EPServiceProvider epService, SupportUpdateListener listener, double px, double py, double x, double y, double width, double height, bool expected) {
            SendAssertWNull(epService, listener, px, py, x, y, width, height, expected);
        }
    
        public static void SendAssertWNull(EPServiceProvider epService, SupportUpdateListener listener, double? px, double? py, double? x, double? y, double? width, double? height, bool? expected) {
            epService.EPRuntime.SendEvent(new ExecSpatialPointRegionQuadTreeEventIndex.MyEventRectangleWithOffset("E", px, py, x, y, width, height));
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
        }
    
        public static string SortJoinProperty(EventBean[] events, string propertyName) {
            var sorted = new SortedDictionary<int, string>();
            foreach (var @event in events) {
                var value = @event.Get(propertyName).ToString();
                var num = int.Parse(value.Substring(1));
                sorted.Put(num, value);
            }

            return string.Join(",", sorted.Values);
        }
    
        public static void SendSpatialPoints(EPServiceProvider epService, int numX, int numY) {
            for (var x = 0; x < numX; x++) {
                for (var y = 0; y < numY; y++) {
                    epService.EPRuntime.SendEvent(new SupportSpatialPoint("P_" + x + "_" + y, (double) x, (double) y));
                }
            }
        }
    
        public static object[][] GetExpected(
            IList<SupportSpatialPoint> points, double x, double y, double width, double height)
        {
            var expected = new SortedSet<string>();
            var boundingBox = new BoundingBox(x, y, x + width, y + height);
            foreach (var p in points) {
                if (boundingBox.ContainsPoint(p.Px.Value, p.Py.Value)) {
                    if (expected.Contains(p.Id)) {
                        Assert.Fail();
                    }
                    expected.Add(p.Id);
                }
            }
            var rows = new Object[expected.Count][];
            var index = 0;
            foreach (var id in expected) {
                rows[index++] = new object[]{id};
            }
            return rows;
        }
    
        public static void SendAssertSpatialAABB(EPServiceProvider epService, SupportUpdateListener listener, int numX, int numY, long deltaMSec)
        {
            var delta = PerformanceObserver.TimeMillis(
                () => {
                    for (var x = 0; x < numX; x++) {
                        for (var y = 0; y < numY; y++) {
                            epService.EPRuntime.SendEvent(new SupportSpatialAABB("", x, y, 0.1, 0.1));
                            listener.AssertOneGetNewAndReset();
                        }
                    }
                });

            Assert.That(delta, Is.LessThan(deltaMSec));
        }
    }
} // end of namespace
