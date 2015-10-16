///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestDependencyGraph
    {
        [Test]
        public void TestGetRootNodes()
        {
            // 1 needs 3 and 4; 2 need 0
            var graph = new DependencyGraph(5, false);
            graph.AddDependency(1, 4);
            graph.AddDependency(1, 3);
            graph.AddDependency(2, 0);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1, 2}, graph.RootNodes);
            Assert.IsNull(graph.FirstCircularDependency);

            // 2 need 0, 3, 4
            graph = new DependencyGraph(5, false);
            graph.AddDependency(2, 0);
            graph.AddDependency(2, 3);
            graph.AddDependency(2, 4);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1, 2}, graph.RootNodes);
            Assert.IsNull(graph.FirstCircularDependency);

            // 2 need 0, 3, 4; 1 needs 2
            graph = new DependencyGraph(5, false);
            graph.AddDependency(2, 0);
            graph.AddDependency(2, 3);
            graph.AddDependency(2, 4);
            graph.AddDependency(1, 2);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1}, graph.RootNodes);
            Assert.IsNull(graph.FirstCircularDependency);

            // circular among 3 nodes
            graph = new DependencyGraph(3, false);
            graph.AddDependency(1, 0);
            graph.AddDependency(2, 1);
            graph.AddDependency(0, 2);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {}, graph.RootNodes);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {0, 2, 1}, graph.FirstCircularDependency.ToArray());

            // circular among 4 nodes
            graph = new DependencyGraph(4, false);
            graph.AddDependency(1, 0);
            graph.AddDependency(2, 0);
            graph.AddDependency(0, 2);
            graph.AddDependency(3, 1);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {3}, graph.RootNodes);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {0, 2}, graph.FirstCircularDependency.ToArray());

            graph.AddDependency(2, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {}, graph.RootNodes);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {0, 2}, graph.FirstCircularDependency.ToArray());

            // circular among 3 nodes
            graph = new DependencyGraph(3, false);
            graph.AddDependency(1, 0);
            graph.AddDependency(0, 1);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {2}, graph.RootNodes);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {0, 1}, graph.FirstCircularDependency.ToArray());

            // circular among 6 nodes
            graph = new DependencyGraph(6, false);
            graph.AddDependency(1, 0);
            graph.AddDependency(0, 2);
            graph.AddDependency(2, 3);
            graph.AddDependency(2, 4);
            graph.AddDependency(4, 0);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1, 5}, graph.RootNodes);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {0, 2, 4}, graph.FirstCircularDependency.ToArray());
        }
    }
}