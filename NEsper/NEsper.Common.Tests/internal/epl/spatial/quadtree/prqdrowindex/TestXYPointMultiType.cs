///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    [TestFixture]
    public class TestXyPointMultiType : AbstractCommonTest
    {
        private void AssertValues(
            string expected,
            XYPointMultiType v)
        {
            var received = v.Multityped is ICollection<object> ? Join((ICollection<object>) v.Multityped) : v.Multityped.ToString();
            Assert.AreEqual(expected, received);
            Assert.AreEqual(v.Count(), expected.SplitCsv().Length);
        }

        private string Join(ICollection<object> collection)
        {
            var joiner = new StringJoiner(",");
            foreach (var value in collection)
            {
                joiner.Add(value.ToString());
            }

            return joiner.ToString();
        }

        [Test, RunInApplicationDomain]
        public void TestAddMultiType()
        {
            var vOne = new XYPointMultiType(10, 20, "X");
            var vTwo = new XYPointMultiType(10, 20, "Y");
            vOne.AddMultiType(vTwo);
            AssertValues("X,Y", vOne);
            AssertValues("Y", vTwo);

            var vThree = new XYPointMultiType(10, 20, "1");
            vThree.AddSingleValue("2");
            vOne.AddMultiType(vThree);
            AssertValues("X,Y,1,2", vOne);
            AssertValues("1,2", vThree);

            var vFour = new XYPointMultiType(10, 20, "X");
            vFour.AddSingleValue("1");
            vFour.AddMultiType(vTwo);
            AssertValues("X,1,Y", vFour);

            var vFive = new XYPointMultiType(10, 20, "A");
            vFive.AddSingleValue("B");
            vFive.AddMultiType(vThree);
            AssertValues("A,B,1,2", vFive);
            vFive.AddSingleValue("C");
            AssertValues("A,B,1,2,C", vFive);
        }

        [Test, RunInApplicationDomain]
        public void TestAddSingleValue()
        {
            var v = new XYPointMultiType(10, 20, "X");
            AssertValues("X", v);

            v.AddSingleValue("Y");
            AssertValues("X,Y", v);

            v.AddSingleValue("Z");
            AssertValues("X,Y,Z", v);
        }

        [Test, RunInApplicationDomain]
        public void TestCollectInto()
        {
            ICollection<object> values = new List<object>();

            var v = new XYPointMultiType(10, 20, "X");
            v.CollectInto(values);
            Assert.AreEqual("X", Join(values));

            values.Clear();
            v.AddSingleValue("Y");
            v.CollectInto(values);
            Assert.AreEqual("X,Y", Join(values));
        }

        [Test, RunInApplicationDomain]
        public void TestInvalidMerge()
        {
            var vOne = new XYPointMultiType(10, 20, "X");
            try
            {
                vOne.AddMultiType(new XYPointMultiType(5, 20, "Y"));
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }

            try
            {
                vOne.AddMultiType(new XYPointMultiType(10, 19, "Y"));
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
} // end of namespace
