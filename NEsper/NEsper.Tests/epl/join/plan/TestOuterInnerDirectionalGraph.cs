///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestOuterInnerDirectionalGraph 
    {
        private OuterInnerDirectionalGraph graph;
    
        [SetUp]
        public void SetUp()
        {
            graph = new OuterInnerDirectionalGraph(4);
        }
    
        [Test]
        public void TestAdd()
        {
            graph.Add(0, 1);
    
            // testing duplicate add
            TryInvalidAdd(0, 1);
    
            // test adding out-of-bounds stream
            TryInvalidAdd(0, 4);
            TryInvalidAdd(4, 0);
            TryInvalidAdd(4, 4);
            TryInvalidAdd(2, -1);
            TryInvalidAdd(-1, 2);
        }
    
        [Test]
        public void TestIsInner()
        {
            graph.Add(0, 1);
            Assert.IsTrue(graph.IsInner(0, 1));
            Assert.IsFalse(graph.IsInner(1, 0));
            Assert.IsFalse(graph.IsInner(2, 0));
            Assert.IsFalse(graph.IsInner(0, 2));
    
            graph.Add(1, 0);
            Assert.IsTrue(graph.IsInner(0, 1));
            Assert.IsTrue(graph.IsInner(1, 0));
    
            graph.Add(2, 0);
            Assert.IsTrue(graph.IsInner(2, 0));
            Assert.IsFalse(graph.IsInner(0, 2));
    
            TryInvalidIsInner(4, 0);
            TryInvalidIsInner(0, 4);
            TryInvalidIsInner(1, 1);
            TryInvalidIsInner(1, -1);
            TryInvalidIsInner(-1, 1);
        }
    
        [Test]
        public void TestIsOuter()
        {
            graph.Add(0, 1);
            Assert.IsTrue(graph.IsOuter(0, 1));
            Assert.IsFalse(graph.IsOuter(1, 0));
            Assert.IsFalse(graph.IsOuter(0, 2));
            Assert.IsFalse(graph.IsOuter(2, 0));
    
            graph.Add(1, 0);
            Assert.IsTrue(graph.IsOuter(1, 0));
            Assert.IsTrue(graph.IsOuter(0, 1));
    
            graph.Add(2, 0);
            Assert.IsTrue(graph.IsOuter(2, 0));
            Assert.IsFalse(graph.IsOuter(0, 2));
    
            TryInvalidIsInner(4, 0);
            TryInvalidIsInner(0, 4);
            TryInvalidIsInner(1, 1);
            TryInvalidIsInner(1, -1);
            TryInvalidIsInner(-1, 1);
        }
    
        [Test]
        public void TestGetInner()
        {
            TryInvalidGetInner(4);
            TryInvalidGetInner(-1);
    
            Assert.IsNull(graph.GetInner(0));
    
            graph.Add(0, 1);
            Assert.IsNull(graph.GetInner(1));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1}, graph.GetInner(0));
            graph.Add(0, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1, 3}, graph.GetInner(0));
            graph.Add(1, 0);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, graph.GetInner(1));
            graph.Add(1, 2);
            graph.Add(1, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0, 2, 3}, graph.GetInner(1));
        }
    
        [Test]
        public void TestGetOuter()
        {
            TryInvalidGetOuter(4);
            TryInvalidGetOuter(-1);
    
            Assert.IsNull(graph.GetOuter(0));
    
            graph.Add(0, 1);
            Assert.IsNull(graph.GetOuter(0));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, graph.GetOuter(1));
            graph.Add(0, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, graph.GetOuter(3));
            graph.Add(1, 0);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, graph.GetOuter(1));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1}, graph.GetOuter(0));
            graph.Add(1, 3);
            graph.Add(2, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0, 1, 2}, graph.GetOuter(3));
        }
    
        private void TryInvalidGetOuter(int stream)
        {
            try
            {
                graph.GetOuter(stream);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }
        }
    
        private void TryInvalidGetInner(int stream)
        {
            try
            {
                graph.GetInner(stream);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }
        }
    
        private void TryInvalidIsInner(int inner, int outer)
        {
            try
            {
                graph.IsInner(inner, outer);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }
        }
    
        private void TryInvalidIsOuter(int inner, int outer)
        {
            try
            {
                graph.IsOuter(outer, inner);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }
        }
    
        private void TryInvalidAdd(int inner, int outer)
        {
            try
            {
                graph.Add(inner, outer);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }
        }
    }
}
