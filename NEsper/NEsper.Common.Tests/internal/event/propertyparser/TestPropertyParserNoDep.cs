///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

using static com.espertech.esper.common.@internal.@event.propertyparser.PropertyParserNoDep;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    [TestFixture]
    public class TestPropertyParserNoDep : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestParse()
        {
            Property property;
            IList<Property> nested;

            property = PropertyParser.ParseAndWalk("a", false);
            Assert.AreEqual("a", ((SimpleProperty) property).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("i[1]", false);
            Assert.AreEqual("i", ((IndexedProperty) property).PropertyNameAtomic);
            Assert.AreEqual(1, ((IndexedProperty) property).Index);

            property = PropertyParser.ParseAndWalk("m('key')", false);
            Assert.AreEqual("m", ((MappedProperty) property).PropertyNameAtomic);
            Assert.AreEqual("key", ((MappedProperty) property).Key);

            property = PropertyParser.ParseAndWalk("a.b[2].c('m')", false);
            nested = ((NestedProperty) property).Properties;
            Assert.AreEqual(3, nested.Count);
            Assert.AreEqual("a", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            Assert.AreEqual("b", ((IndexedProperty) nested[1]).PropertyNameAtomic);
            Assert.AreEqual(2, ((IndexedProperty) nested[1]).Index);
            Assert.AreEqual("c", ((MappedProperty) nested[2]).PropertyNameAtomic);
            Assert.AreEqual("m", ((MappedProperty) nested[2]).Key);

            property = PropertyParser.ParseAndWalk("a", true);
            Assert.AreEqual("a", ((DynamicSimpleProperty) property).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("`order`.p0", false);
            nested = ((NestedProperty) property).Properties;
            Assert.AreEqual(2, nested.Count);
            Assert.AreEqual("order", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            Assert.AreEqual("p0", ((SimpleProperty) nested[1]).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("`jim's strings`.p0", false);
            nested = ((NestedProperty) property).Properties;
            Assert.AreEqual(2, nested.Count);
            Assert.AreEqual("jim's strings", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            Assert.AreEqual("p0", ((SimpleProperty) nested[1]).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("`children's books`[0]", false);
            IndexedProperty indexed = (IndexedProperty) property;
            Assert.AreEqual(0, indexed.Index);
            Assert.AreEqual("children's books", indexed.PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("x\\.y", false);
            Assert.AreEqual("x.y", ((SimpleProperty) property).PropertyNameAtomic);
            property = PropertyParser.ParseAndWalk("x\\.\\.y", false);
            Assert.AreEqual("x..y", ((SimpleProperty) property).PropertyNameAtomic);
        }

        [Test, RunInApplicationDomain]
        public void TestParseMapKey()
        {
            Assert.AreEqual("a", TryKey("a"));
        }

        [Test, RunInApplicationDomain]
        public void TestParseMappedProp()
        {
            MappedPropertyParseResult result = ParseMappedProperty("a.b('c')");
            Assert.AreEqual("a", result.ClassName);
            Assert.AreEqual("b", result.MethodName);
            Assert.AreEqual("c", result.ArgString);

            result = ParseMappedProperty("SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))')");
            Assert.AreEqual("SupportStaticMethodLib", result.ClassName);
            Assert.AreEqual("DelimitPipe", result.MethodName);
            Assert.AreEqual("POLYGON ((100.0 100, \", 100 100, 400 400))", result.ArgString);

            result = ParseMappedProperty("a.b.c.d.e('f.g.h,u.h')");
            Assert.AreEqual("a.b.c.d", result.ClassName);
            Assert.AreEqual("e", result.MethodName);
            Assert.AreEqual("f.g.h,u.h", result.ArgString);

            result = ParseMappedProperty("a.b.c.d.E(\"hfhf f f f \")");
            Assert.AreEqual("a.b.c.d", result.ClassName);
            Assert.AreEqual("E", result.MethodName);
            Assert.AreEqual("hfhf f f f ", result.ArgString);

            result = ParseMappedProperty("c.d.getEnumerationSource(\"kf\"kf'kf\")");
            Assert.AreEqual("c.d", result.ClassName);
            Assert.AreEqual("getEnumerationSource", result.MethodName);
            Assert.AreEqual("kf\"kf'kf", result.ArgString);

            result = ParseMappedProperty("c.d.getEnumerationSource('kf\"kf'kf\"')");
            Assert.AreEqual("c.d", result.ClassName);
            Assert.AreEqual("getEnumerationSource", result.MethodName);
            Assert.AreEqual("kf\"kf'kf\"", result.ArgString);

            result = ParseMappedProperty("f('a')");
            Assert.AreEqual(null, result.ClassName);
            Assert.AreEqual("f", result.MethodName);
            Assert.AreEqual("a", result.ArgString);

            result = ParseMappedProperty("f('.')");
            Assert.AreEqual(null, result.ClassName);
            Assert.AreEqual("f",  result.MethodName);
            Assert.AreEqual(".", result.ArgString);

            Assert.IsNull(ParseMappedProperty("('a')"));
            Assert.IsNull(ParseMappedProperty(""));
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
