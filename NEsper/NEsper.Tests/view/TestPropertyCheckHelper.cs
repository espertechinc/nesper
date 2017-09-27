///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.view
{
    [TestFixture]
    public class TestPropertyCheckHelper 
    {
        [Test]
        public void TestCheckNumeric()
        {
            EventType mySchema = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
    
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "dummy") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Symbol") != null);
    
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Volume") == null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Price") == null);
    
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "dummy", "dummy2") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Symbol", "dummy2") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Symbol", "Price") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Price", "dummy") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "dummy", "Price") != null);
    
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Price", "Price") == null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Price", "Volume") == null);
            Assert.IsTrue(PropertyCheckHelper.CheckNumeric(mySchema, "Volume", "Price") == null);
        }
    
        [Test]
        public void TestCheckLong()
        {
            EventType mySchema = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
    
            Assert.AreEqual(null, PropertyCheckHelper.CheckLong(mySchema, "LongPrimitive"));
            Assert.AreEqual(null, PropertyCheckHelper.CheckLong(mySchema, "LongBoxed"));
            Assert.AreEqual(null, PropertyCheckHelper.CheckLong(mySchema, "LongBoxed"));
            Assert.IsTrue(PropertyCheckHelper.CheckLong(mySchema, "dummy") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckLong(mySchema, "IntPrimitive") != null);
            Assert.IsTrue(PropertyCheckHelper.CheckLong(mySchema, "DoubleBoxed") != null);
        }
    
        [Test]
        public void TestFieldExist()
        {
            EventType mySchema = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
    
            Assert.AreEqual(null, PropertyCheckHelper.Exists(mySchema, "LongPrimitive"));
            Assert.IsTrue(PropertyCheckHelper.Exists(mySchema, "dummy") != null);
        }
    
        [Test]
        public void Test2FieldExist()
        {
            EventType mySchema = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
    
            Assert.AreEqual(null, PropertyCheckHelper.Exists(mySchema, "LongPrimitive", "LongBoxed"));
            Assert.IsTrue(PropertyCheckHelper.Exists(mySchema, "dummy", "LongPrimitive") != null);
            Assert.IsTrue(PropertyCheckHelper.Exists(mySchema, "LongPrimitive", "dummy") != null);
            Assert.IsTrue(PropertyCheckHelper.Exists(mySchema, "dummy", "dummy") != null);
        }
    }
}
