///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestBitWiseOperators 
    {
        private const byte FIRST_EVENT = 1;
        private const short SECOND_EVENT = 2;
        private const int THIRD_EVENT = FIRST_EVENT | SECOND_EVENT;
        private const long FOURTH_EVENT = 4;
        private const bool FITH_EVENT = false;
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestBitWiseOperators_OM()
        {
            String viewExpr = "select BytePrimitive&ByteBoxed as myFirstProperty, " +
                    "ShortPrimitive|ShortBoxed as mySecondProperty, " +
                    "IntPrimitive|IntBoxed as myThirdProperty, " +
                    "LongPrimitive^LongBoxed as myFourthProperty, " +
                    "BoolPrimitive&BoolBoxed as myFifthProperty " +
                    "from " + typeof(SupportBean).FullName + "#length(3)";
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.BinaryAnd().Add("BytePrimitive").Add("ByteBoxed"), "myFirstProperty")
                .Add(Expressions.BinaryOr().Add("ShortPrimitive").Add("ShortBoxed"), "mySecondProperty")
                .Add(Expressions.BinaryOr().Add("IntPrimitive").Add("IntBoxed"), "myThirdProperty")
                .Add(Expressions.BinaryXor().Add("LongPrimitive").Add("LongBoxed"), "myFourthProperty")
                .Add(Expressions.BinaryAnd().Add("BoolPrimitive").Add("BoolBoxed"), "myFifthProperty");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView("length", Expressions.Constant(3)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(viewExpr, model.ToEPL());
    
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
    
            RunBitWiseOperators();
        }
    
        [Test]
        public void TestWithBitWiseOperators()
        {
            SetUpBitWiseStmt();
            _listener.Reset();
    
            RunBitWiseOperators();
        }
    
        private void RunBitWiseOperators()
        {
            SendEvent(FIRST_EVENT, FIRST_EVENT, SECOND_EVENT, SECOND_EVENT,
                    FIRST_EVENT, THIRD_EVENT, 3L, FOURTH_EVENT,
                    FITH_EVENT, FITH_EVENT);
    
            EventBean received = _listener.GetAndResetLastNewData()[0];
            Assert.AreEqual((byte) 1, (received.Get("myFirstProperty")));
            Assert.IsTrue(((short) (received.Get("mySecondProperty")) & SECOND_EVENT) == SECOND_EVENT);
            Assert.IsTrue(((int) (received.Get("myThirdProperty")) & FIRST_EVENT) == FIRST_EVENT);
            Assert.AreEqual(7L, (received.Get("myFourthProperty")));
            Assert.AreEqual(false, (received.Get("myFifthProperty")));
        }
    
        private EPStatement SetUpBitWiseStmt()
        {
            String viewExpr = "select (BytePrimitive & ByteBoxed) as myFirstProperty, " +
                    "(ShortPrimitive | ShortBoxed) as mySecondProperty, " +
                    "(IntPrimitive | IntBoxed) as myThirdProperty, " +
                    "(LongPrimitive ^ LongBoxed) as myFourthProperty, " +
                    "(BoolPrimitive & BoolBoxed) as myFifthProperty " +
                    " from " + typeof(SupportBean).FullName + "#length(3) ";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
    
            EventType type = selectTestView.EventType;
            Assert.AreEqual(typeof(byte?), type.GetPropertyType("myFirstProperty"));
            Assert.AreEqual(typeof(short?), type.GetPropertyType("mySecondProperty"));
            Assert.AreEqual(typeof(int?), type.GetPropertyType("myThirdProperty"));
            Assert.AreEqual(typeof(long?), type.GetPropertyType("myFourthProperty"));
            Assert.AreEqual(typeof(bool?), type.GetPropertyType("myFifthProperty"));
    
            return selectTestView;
        }

        protected void SendEvent(byte bytePrimitive,
                                 byte? byteBoxed,
                                 short shortPrimitive,
                                 short? shortBoxed,
                                 int intPrimitive,
                                 int? intBoxed,
                                 long longPrimitive,
                                 long? longBoxed,
                                 bool boolPrimitive,
                                 bool? boolBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.BytePrimitive = bytePrimitive;
            bean.ByteBoxed = byteBoxed;
            bean.ShortPrimitive = shortPrimitive;
            bean.ShortBoxed = shortBoxed;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            bean.BoolPrimitive = boolPrimitive;
            bean.BoolBoxed = boolBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
