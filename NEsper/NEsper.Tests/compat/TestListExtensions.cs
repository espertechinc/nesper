///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class TestListExtensions
    {
        [Test]
        public void WhenNullShouldReturnNull()
        {
            Assert.IsNull(ListExtensions.AsObjectList(null));
        }

        [Test]
        public void WhenListObjectReturnSame()
        {
            List<object> sample = new List<object>();
            Assert.AreSame(sample, ListExtensions.AsObjectList(sample));
        }

        [Test]
        public void WhenListGenericReturnUnmasked()
        {
            List<string> sample = new List<string>();
            sample.Add("A");
            sample.Add("B");

            IList<object> result = ListExtensions.AsObjectList(sample);
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(sample.Count));
        }
    }
}
