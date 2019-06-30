///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.property
{
    [TestFixture]
    public class TestPropertyParser : CommonTest
    {
        [Test]
        public void TestParse()
        {
            Property property = PropertyParser.ParseAndWalk("a", false);
            Assert.AreEqual("a", ((SimpleProperty) property).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("i[1]", false);
            Assert.AreEqual("i", ((IndexedProperty) property).PropertyNameAtomic);
            Assert.AreEqual(1, ((IndexedProperty) property).Index);

            property = PropertyParser.ParseAndWalk("m('key')", false);
            Assert.AreEqual("m", ((MappedProperty) property).PropertyNameAtomic);
            Assert.AreEqual("key", ((MappedProperty) property).Key);

            property = PropertyParser.ParseAndWalk("a.b[2].c('m')", false);
            IList<Property> nested = ((NestedProperty) property).Properties;
            Assert.AreEqual(3, nested.Count);
            Assert.AreEqual("a", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            Assert.AreEqual("b", ((IndexedProperty) nested[1]).PropertyNameAtomic);
            Assert.AreEqual(2, ((IndexedProperty) nested[1]).Index);
            Assert.AreEqual("c", ((MappedProperty) nested[2]).PropertyNameAtomic);
            Assert.AreEqual("m", ((MappedProperty) nested[2]).Key);

            property = PropertyParser.ParseAndWalk("a", true);
            Assert.AreEqual("a", ((DynamicSimpleProperty) property).PropertyNameAtomic);
        }

        [Test]
        public void TestParseMapKey()
        {
            Assert.AreEqual("a", TryKey("a"));
        }

        private string TryKey(string key)
        {
            string propertyName = "m(\"" + key + "\")";
            log.Debug(".tryKey propertyName=" + propertyName + " key=" + key);
            Property property = PropertyParser.ParseAndWalk(propertyName, false);
            return ((MappedProperty) property).Key;
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace