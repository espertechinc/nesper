///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.view
{
    [TestFixture]
    public class TestViewSpec
    {
        [Test]
        public void TestEquals()
        {
            var c_0 = new[] {typeof(string)};
            var s_0_0 = new[] {"\"Symbol\""};
            var s_0_1 = new[] {"\"Price\""};

            var c_1 = new[] {typeof(string), typeof(long?)};
            var s_1_0 = new[] {"\"Symbol\"", "1"};
            var s_1_1 = new[] {"\"Price\"", "1"};
            var s_1_2 = new[] {"\"Price\"", "2"};
            var s_1_3 = new[] {"\"Price\"", "1"};

            var c_2 = new[] {typeof(bool?), typeof(string), typeof(long?)};
            var s_2_0 = new[] {"true", "\"Symbol\"", "1"};
            var s_2_1 = new[] {"true", "\"Price\"", "1"};
            var s_2_2 = new[] {"true", "\"Price\"", "2"};
            var s_2_3 = new[] {"false", "\"Price\"", "1"};

            IDictionary<int, ViewSpec> specs = new Dictionary<int, ViewSpec>();
            specs.Put(1, SupportViewSpecFactory.MakeSpec("ext", "sort", null, null));
            specs.Put(2, SupportViewSpecFactory.MakeSpec("std", "sum", null, null));
            specs.Put(3, SupportViewSpecFactory.MakeSpec("ext", "sort", null, null));
            specs.Put(4, SupportViewSpecFactory.MakeSpec("ext", "sort", c_0, s_0_0));
            specs.Put(5, SupportViewSpecFactory.MakeSpec("ext", "sort", c_0, s_0_0));
            specs.Put(6, SupportViewSpecFactory.MakeSpec("ext", "sort", c_0, s_0_1));
            specs.Put(7, SupportViewSpecFactory.MakeSpec("ext", "sort", c_1, s_1_0));
            specs.Put(8, SupportViewSpecFactory.MakeSpec("ext", "sort", c_1, s_1_1));
            specs.Put(9, SupportViewSpecFactory.MakeSpec("ext", "sort", c_1, s_1_2));
            specs.Put(10, SupportViewSpecFactory.MakeSpec("ext", "sort", c_1, s_1_3));
            specs.Put(11, SupportViewSpecFactory.MakeSpec("ext", "sort", c_2, s_2_0));
            specs.Put(12, SupportViewSpecFactory.MakeSpec("ext", "sort", c_2, s_2_1));
            specs.Put(13, SupportViewSpecFactory.MakeSpec("ext", "sort", c_2, s_2_2));
            specs.Put(14, SupportViewSpecFactory.MakeSpec("ext", "sort", c_2, s_2_3));

            IDictionary<int, int> matches = new Dictionary<int, int>();
            matches.Put(1, 3);
            matches.Put(3, 1);
            matches.Put(4, 5);
            matches.Put(5, 4);
            matches.Put(8, 10);
            matches.Put(10, 8);

            // Compare each against each
            foreach (var entryOut in specs) {
                foreach (var entryIn in specs) {
                    bool result = entryOut.Value.Equals(entryIn.Value);

                    if (Equals(entryOut, entryIn)) {
                        Assert.IsTrue(result);
                        continue;
                    }


                    String message = "Comparing " + entryIn.Key + "=" + entryIn.Value + "   and   " + entryOut.Key + "=" +
                                     entryOut.Value;
                    if ((matches.ContainsKey(entryOut.Key)) &&
                        (matches.Get(entryOut.Key) == entryIn.Key)) {
                        Assert.IsTrue(result, message);
                    }
                    else {
                        Assert.IsFalse(result, message);
                    }
                }
            }
        }
    }
}
