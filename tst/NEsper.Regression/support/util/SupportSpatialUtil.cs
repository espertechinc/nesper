///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.util
{
	public class SupportSpatialUtil
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SupportSpatialUtil));

		public static IList<SupportSpatialEventRectangle> RandomRectangles(
			Random random,
			int numPoints,
			double x,
			double y,
			double width,
			double height)
		{
			IList<SupportSpatialEventRectangle> rectangles = new List<SupportSpatialEventRectangle>();
			for (var i = 0; i < numPoints; i++) {
				var rx = random.NextDouble() * width + x;
				var ry = random.NextDouble() * height + y;
				var rw = random.NextDouble() * width * 0.3;
				var rh = random.NextDouble() * height * 0.3;
				rectangles.Add(new SupportSpatialEventRectangle("R" + i, rx, ry, rw, rh));
			}

			return rectangles;
		}

		public static void AssertAllRectangles(
			RegressionEnvironment env,
			ICollection<SupportSpatialEventRectangle> expected,
			double x,
			double y,
			double width,
			double height)
		{
			env.SendEventBean(new SupportSpatialAABB("", x, y, width, height));
			var events = env.Listener("s0").GetAndResetLastNewData();
			if (events == null || events.Length == 0) {
				Assert.IsTrue(expected.IsEmpty());
				return;
			}

			Assert.AreEqual(expected.Count, events.Length);
			ISet<string> received = new HashSet<string>();
			foreach (var @event in events) {
				received.Add(@event.Get("c0").ToString());
			}

			Assert.AreEqual(expected.Count, received.Count);
			foreach (var r in expected) {
				Assert.IsTrue(received.Contains(r.Id));
			}
		}

		public static void AssertBBTreeRectangles(
			RegressionEnvironment env,
			string stmtName,
			BoundingBox.BoundingBoxNode bbtree,
			IList<SupportSpatialEventRectangle> rectangles)
		{
			AssertBBRectangles(env, stmtName, bbtree.bb, rectangles);
			if (bbtree.nw != null) {
				AssertBBTreeRectangles(env, stmtName, bbtree.nw, rectangles);
			}

			if (bbtree.ne != null) {
				AssertBBTreeRectangles(env, stmtName, bbtree.ne, rectangles);
			}

			if (bbtree.sw != null) {
				AssertBBTreeRectangles(env, stmtName, bbtree.sw, rectangles);
			}

			if (bbtree.se != null) {
				AssertBBTreeRectangles(env, stmtName, bbtree.se, rectangles);
			}
		}

		public static void AddSendRectangle(
			RegressionEnvironment env,
			IList<SupportSpatialEventRectangle> rectangles,
			string id,
			double x,
			double y,
			double width,
			double height)
		{
			var rectangle = new SupportSpatialEventRectangle(id, x, y, width, height);
			rectangles.Add(rectangle);
			env.SendEventBean(rectangle);
		}

		public static IList<SupportSpatialPoint> RandomPoints(
			Random random,
			int numPoints,
			double x,
			double y,
			double width,
			double height)
		{
			IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
			for (var i = 0; i < numPoints; i++) {
				var px = random.NextDouble() * width + x;
				var py = random.NextDouble() * height + y;
				points.Add(new SupportSpatialPoint("P" + i, px, py));
			}

			return points;
		}

		public static void AssertBBTreePoints(
			RegressionEnvironment env,
			BoundingBox.BoundingBoxNode bbtree,
			IList<SupportSpatialPoint> points)
		{
			AssertBBPoints(env, bbtree.bb, points);
			if (bbtree.nw != null) {
				AssertBBTreePoints(env, bbtree.nw, points);
			}

			if (bbtree.ne != null) {
				AssertBBTreePoints(env, bbtree.ne, points);
			}

			if (bbtree.sw != null) {
				AssertBBTreePoints(env, bbtree.sw, points);
			}

			if (bbtree.se != null) {
				AssertBBTreePoints(env, bbtree.se, points);
			}
		}

		public static void AddSendPoint(
			RegressionEnvironment env,
			IList<SupportSpatialPoint> points,
			string id,
			double x,
			double y)
		{
			var point = new SupportSpatialPoint(id, x, y);
			points.Add(point);
			env.SendEventBean(point);
		}

		public static void AssertAllPoints(
			RegressionEnvironment env,
			ICollection<SupportSpatialPoint> expected,
			double x,
			double y,
			double width,
			double height)
		{
			env.SendEventBean(new SupportSpatialAABB("", x, y, width, height));
			var events = env.Listener("s0").GetAndResetLastNewData();
			if (events == null || events.Length == 0) {
				Assert.IsTrue(expected.IsEmpty());
				return;
			}

			Assert.AreEqual(expected.Count, events.Length);
			ISet<string> received = new HashSet<string>();
			foreach (var @event in events) {
				received.Add(@event.Get("c0").ToString());
			}

			Assert.AreEqual(expected.Count, received.Count);
			foreach (var p in expected) {
				Assert.IsTrue(received.Contains(p.Id));
			}
		}

		public static void SendAddPoint(
			RegressionEnvironment env,
			IList<SupportSpatialPoint> points,
			string id,
			double x,
			double y)
		{
			var point = new SupportSpatialPoint(id, x, y);
			points.Add(point);
			env.SendEventBean(point);
		}

		public static void AssertBBPoints(
			RegressionEnvironment env,
			BoundingBox bb,
			IList<SupportSpatialPoint> points)
		{
			env.SendEventBean(new SupportSpatialAABB("", bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY));
			env.AssertListener(
				"s0",
				listener => {
					var received = SortJoinProperty(listener.GetAndResetLastNewData(), "c0");
					var expected = SortGetExpectedPoints(bb, points);
					Assert.AreEqual(expected, received);
				});
		}

		public static string SortGetExpectedPoints(
			BoundingBox bb,
			IList<SupportSpatialPoint> points)
		{
			var joiner = new StringJoiner(",");
			foreach (var point in points) {
				if (bb.ContainsPoint(point.Px.Value, point.Py.Value)) {
					joiner.Add(point.Id);
				}
			}

			return joiner.ToString();
		}

		public static void SendAddRectangle(
			RegressionEnvironment env,
			IList<SupportSpatialEventRectangle> rectangles,
			string id,
			double x,
			double y,
			double width,
			double height)
		{
			var rectangle = new SupportSpatialEventRectangle(id, x, y, width, height);
			rectangles.Add(rectangle);
			env.SendEventBean(rectangle);
		}

		public static void AssertBBRectangles(
			RegressionEnvironment env,
			string stmtName,
			BoundingBox bb,
			IList<SupportSpatialEventRectangle> rectangles)
		{
			env.SendEventBean(new SupportSpatialAABB("", bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY));
			env.AssertListener(
				stmtName,
				listener => {
					var received = SortJoinProperty(listener.GetAndResetLastNewData(), "c0");
					var expected = SortGetExpectedRectangles(bb, rectangles);
					if (!received.Equals(expected)) {
						Log.Error("Expected: " + expected);
						Log.Error("Received: " + received);
					}

					Assert.AreEqual(expected, received);
				});
		}

		public static void SendEventRectangle(
			RegressionEnvironment env,
			string id,
			double x,
			double y,
			double width,
			double height)
		{
			env.SendEventBean(new SupportSpatialEventRectangle(id, x, y, width, height));
		}

		public static void AssertRectanglesSingleValueAssertS0S1(
			RegressionEnvironment env,
			SupportUpdateListener listener,
			IList<BoundingBox> rectangles,
			params string[] matches)
		{
			for (var i = 0; i < rectangles.Count; i++) {
				var box = rectangles[i];
				SendRectangle(env, "R" + box.ToString(), box.MinX, box.MinY, box.MaxX - box.MinX, box.MaxY - box.MinY);
				var c0 = listener.AssertOneGetNewAndReset().Get("c0").ToString();
				Assert.AreEqual(matches[i], c0, "for box " + i);
			}
		}

		public static void AssertRectanglesSingleValueAssertS0(
			RegressionEnvironment env,
			IList<BoundingBox> rectangles,
			params string[] matches)
		{
			for (var i = 0; i < rectangles.Count; i++) {
				var box = rectangles[i];
				SendRectangle(env, "R" + box.ToString(), box.MinX, box.MinY, box.MaxX - box.MinX, box.MaxY - box.MinY);
				var index = i;
				env.AssertEventNew(
					"s0",
					@event => {
						var c0 = @event.Get("c0").ToString();
						Assert.AreEqual(matches[index], c0, "for box " + index);
					});
			}
		}

		public static void AssertRectanglesManyRow(
			RegressionEnvironment env,
			IList<BoundingBox> rectangles,
			params string[] matches)
		{
			for (var i = 0; i < rectangles.Count; i++) {
				var box = rectangles[i];
				SendRectangle(env, "R" + box.ToString(), box.MinX, box.MinY, box.MaxX - box.MinX, box.MaxY - box.MinY);
				var index = i;
				env.AssertListener(
					"s0",
					listener => {
						if (matches[index] == null) {
							if (listener.IsInvoked) {
								Assert.Fail(
									"Unexpected output for box " +
									index +
									": " +
									SortJoinProperty(listener.GetAndResetLastNewData(), "c0"));
							}
						}
						else {
							if (!listener.IsInvoked) {
								Assert.Fail("No output for box " + index);
							}

							Assert.AreEqual(matches[index], SortJoinProperty(listener.GetAndResetLastNewData(), "c0"));
						}
					});
			}
		}

		public static void SendPoint(
			RegressionEnvironment env,
			string id,
			double x,
			double y)
		{
			env.SendEventBean(new SupportSpatialPoint(id, x, y));
		}

		public static void SendPoint(
			RegressionEnvironment env,
			string id,
			double x,
			double y,
			string category)
		{
			env.SendEventBean(new SupportSpatialPoint(id, x, y, category));
		}

		public static void SendRectangle(
			RegressionEnvironment env,
			string id,
			double x,
			double y,
			double width,
			double height)
		{
			env.SendEventBean(new SupportSpatialAABB(id, x, y, width, height));
		}

		public static void SendAssert(
			RegressionEnvironment env,
			double px,
			double py,
			double x,
			double y,
			double width,
			double height,
			bool expected)
		{
			SendAssertWNull(env, px, py, x, y, width, height, expected);
		}

		public static void SendAssertWNull(
			RegressionEnvironment env,
			double? px,
			double? py,
			double? x,
			double? y,
			double? width,
			double? height,
			bool? expected)
		{
			env.SendEventBean(new SupportEventRectangleWithOffset("E", px, py, x, y, width, height));
			env.AssertEqualsNew("s0", "c0", expected);
		}

		public static string SortJoinProperty(
			EventBean[] events,
			string propertyName)
		{
			var sorted = new SortedDictionary<int, string>();
			if (events != null) {
				foreach (var @event in events) {
					var value = @event.Get(propertyName).ToString();
					var num = int.Parse(value.Substring(1));
					sorted.Put(num, value);
				}
			}

			var joiner = new StringJoiner(",");
			foreach (var data in sorted.Values) {
				joiner.Add(data);
			}

			return joiner.ToString();
		}

		public static void SendSpatialPoints(
			RegressionEnvironment env,
			int numX,
			int numY)
		{
			for (var x = 0; x < numX; x++) {
				for (var y = 0; y < numY; y++) {
					env.SendEventBean(new SupportSpatialPoint("P_" + x + "_" + y, (double)x, (double)y));
				}
			}
		}

		public static object[][] GetExpected(
			IList<SupportSpatialPoint> points,
			double x,
			double y,
			double width,
			double height)
		{
			ISet<string> expected = new SortedSet<string>();
			var boundingBox = new BoundingBox(x, y, x + width, y + height);
			foreach (var p in points) {
				if (boundingBox.ContainsPoint(p.Px.Value, p.Py.Value)) {
					if (expected.Contains(p.Id)) {
						Assert.Fail();
					}

					expected.Add(p.Id);
				}
			}

			var rows = new object[expected.Count][];
			var index = 0;
			foreach (var id in expected) {
				rows[index++] = new object[] { id };
			}

			return rows;
		}

		public static void SendAssertSpatialAABB(
			RegressionEnvironment env,
			int numX,
			int numY,
			long deltaMSec)
		{
			var start = PerformanceObserver.MilliTime;
			for (var x = 0; x < numX; x++) {
				for (var y = 0; y < numY; y++) {
					env.SendEventBean(new SupportSpatialAABB("", x, y, 0.1, 0.1));
					env.AssertEventNew("s0", @event => { });
				}
			}

			var delta = PerformanceObserver.MilliTime - start;
			Assert.That(delta, Is.LessThan(deltaMSec));
		}

		public static void SendAssertSpatialAABB(
			RegressionEnvironment env,
			SupportListener listener,
			int numX,
			int numY,
			long deltaMSec)
		{
			var start = PerformanceObserver.MilliTime;
			for (var x = 0; x < numX; x++) {
				for (var y = 0; y < numY; y++) {
					env.SendEventBean(new SupportSpatialAABB("", x, y, 0.1, 0.1));
					listener.AssertOneGetNewAndReset();
				}
			}

			var delta = PerformanceObserver.MilliTime - start;
			Assert.That(delta, Is.LessThan(deltaMSec));
		}

		public static string SortGetExpectedRectangles(
			BoundingBox bb,
			IList<SupportSpatialEventRectangle> rectangles)
		{
			var joiner = new StringJoiner(",");
			foreach (var rect in rectangles) {
				if (bb.IntersectsBoxIncludingEnd(rect.X.Value, rect.Y.Value, rect.Width.Value, rect.Height.Value)) {
					joiner.Add(rect.Id);
				}
			}

			return joiner.ToString();
		}

		public static string BuildDeleteQueryWithInClause(
			string infraName,
			string field,
			IList<string> idList)
		{
			var query = new StringBuilder();
			query.Append("delete from ").Append(infraName).Append(" where ").Append(field).Append(" in (");
			var delimiter = "";
			foreach (var id in idList) {
				query.Append(delimiter).Append('\'').Append(id).Append("\'");
				delimiter = ",";
			}

			query.Append(")");
			return query.ToString();
		}
	}
} // end of namespace
