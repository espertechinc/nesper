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
        private OuterInnerDirectionalGraph _graph;
    
        [SetUp]
        public void SetUp()
        {
            _graph = new OuterInnerDirectionalGraph(4);
        }
    
        [Test]
        public void TestAdd()
        {
            _graph.Add(0, 1);
    
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
            _graph.Add(0, 1);
            Assert.IsTrue(_graph.IsInner(0, 1));
            Assert.IsFalse(_graph.IsInner(1, 0));
            Assert.IsFalse(_graph.IsInner(2, 0));
            Assert.IsFalse(_graph.IsInner(0, 2));
    
            _graph.Add(1, 0);
            Assert.IsTrue(_graph.IsInner(0, 1));
            Assert.IsTrue(_graph.IsInner(1, 0));
    
            _graph.Add(2, 0);
            Assert.IsTrue(_graph.IsInner(2, 0));
            Assert.IsFalse(_graph.IsInner(0, 2));
    
            TryInvalidIsInner(4, 0);
            TryInvalidIsInner(0, 4);
            TryInvalidIsInner(1, 1);
            TryInvalidIsInner(1, -1);
            TryInvalidIsInner(-1, 1);
        }
    
        [Test]
        public void TestIsOuter()
        {
            _graph.Add(0, 1);
            Assert.IsTrue(_graph.IsOuter(0, 1));
            Assert.IsFalse(_graph.IsOuter(1, 0));
            Assert.IsFalse(_graph.IsOuter(0, 2));
            Assert.IsFalse(_graph.IsOuter(2, 0));
    
            _graph.Add(1, 0);
            Assert.IsTrue(_graph.IsOuter(1, 0));
            Assert.IsTrue(_graph.IsOuter(0, 1));
    
            _graph.Add(2, 0);
            Assert.IsTrue(_graph.IsOuter(2, 0));
            Assert.IsFalse(_graph.IsOuter(0, 2));
    
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
    
            Assert.IsNull(_graph.GetInner(0));
    
            _graph.Add(0, 1);
            Assert.IsNull(_graph.GetInner(1));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1}, _graph.GetInner(0));
            _graph.Add(0, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1, 3}, _graph.GetInner(0));
            _graph.Add(1, 0);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, _graph.GetInner(1));
            _graph.Add(1, 2);
            _graph.Add(1, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0, 2, 3}, _graph.GetInner(1));
        }
    
        [Test]
        public void TestGetOuter()
        {
            TryInvalidGetOuter(4);
            TryInvalidGetOuter(-1);
    
            Assert.IsNull(_graph.GetOuter(0));
    
            _graph.Add(0, 1);
            Assert.IsNull(_graph.GetOuter(0));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, _graph.GetOuter(1));
            _graph.Add(0, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, _graph.GetOuter(3));
            _graph.Add(1, 0);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0}, _graph.GetOuter(1));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1}, _graph.GetOuter(0));
            _graph.Add(1, 3);
            _graph.Add(2, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {0, 1, 2}, _graph.GetOuter(3));
        }
    
        private void TryInvalidGetOuter(int stream)
        {
            try
            {
                _graph.GetOuter(stream);
                Assert.Fail();
            }
            catch (Exception)
            {
                // expected
            }
        }
    
        private void TryInvalidGetInner(int stream)
        {
            try
            {
                _graph.GetInner(stream);
                Assert.Fail();
            }
            catch (Exception)
            {
                // expected
            }
        }
    
        private void TryInvalidIsInner(int inner, int outer)
        {
            try
            {
                _graph.IsInner(inner, outer);
                Assert.Fail();
            }
            catch (Exception)
            {
                // expected
            }
        }
    
        private void TryInvalidIsOuter(int inner, int outer)
        {
            try
            {
                _graph.IsOuter(outer, inner);
                Assert.Fail();
            }
            catch (Exception)
            {
                // expected
            }
        }
    
        private void TryInvalidAdd(int inner, int outer)
        {
            try
            {
                _graph.Add(inner, outer);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
}
