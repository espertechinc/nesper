///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestSuperEnumerator : AbstractCommonTest
    {
        private IEnumerator<string> Make(string csv)
        {
            if (csv == null || csv.Length == 0) {
                return EnumerationHelper.Empty<string>();
            }

            var fields = csv.SplitCsv();
            return Arrays.AsList(fields).GetEnumerator();
        }

        [Test]
        public void TestEmpty()
        {
            var enumerator = SuperEnumerator.For(Make(null), Make(null));
            ClassicAssert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void TestFlow()
        {
            var enumerator = SuperEnumerator.For(Make("a"), Make(null));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a"}, enumerator);

            enumerator = SuperEnumerator.For(Make("a,b"), Make(null));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", "b"}, enumerator);

            enumerator = SuperEnumerator.For(Make("a"), Make("b"));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", "b"}, enumerator);

            enumerator = SuperEnumerator.For(Make(null), Make("a,b"));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", "b"}, enumerator);
        }
    }
} // end of namespace
