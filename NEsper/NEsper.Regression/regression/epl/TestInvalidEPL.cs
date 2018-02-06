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
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInvalidEPL
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestInvalidFuncParams()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            SupportMessageAssertUtil.TryInvalid(_epService, "select count(TheString, TheString, TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'count(TheString,TheString,TheString)': The 'count' function expects at least 1 and up to 2 parameters");

            SupportMessageAssertUtil.TryInvalid(_epService, "select leaving(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'leaving(TheString)': The 'leaving' function expects no parameters");
        }

        [Test]
        public void TestInvalidSyntax()
        {
            String exceptionText = GetSyntaxExceptionEPL("select * from *");
            Assert.AreEqual("Incorrect syntax near '*' at line 1 column 14, please check the from clause [select * from *]", exceptionText);

            exceptionText = GetSyntaxExceptionEPL("select * from SupportBean a where a.IntPrimitive between r.start and r.end");
            Assert.AreEqual("Incorrect syntax near 'start' (a reserved keyword) at line 1 column 59, please check the where clause [select * from SupportBean a where a.IntPrimitive between r.start and r.end]", exceptionText);

            SupportMessageAssertUtil.TryInvalid(
                _epService, "select * from System.Object(1=2=3)",
                "Failed to validate filter expression '1=2': Invalid use of equals, expecting left-hand side and right-hand side but received 3 expressions");
        }

        [Test]
        public void TestLongTypeConstant()
        {
            String stmtText = "select 2512570244 as value from " + typeof(SupportBean).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(2512570244L, _listener.AssertOneGetNewAndReset().Get("value"));
        }

        [Test]
        public void TestDifferentJoins()
        {
            try
            {
                _epService.EPAdministrator.CreateEPL("select *");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: The from-clause is required but has not been specified [select *]", ex.Message);
            }

            String streamDef = "select * from " +
                    typeof(SupportBean).FullName + "#length(3) as sa," +
                    typeof(SupportBean).FullName + "#length(3) as sb" +
                    " where ";

            String streamDefTwo = "select * from " +
                    typeof(SupportBean).FullName + "#length(3)," +
                    typeof(SupportMarketDataBean).FullName + "#length(3)" +
                    " where ";

            TryInvalid(streamDef + "sa.IntPrimitive = sb.TheString");
            TryValid(streamDef + "sa.IntPrimitive = sb.IntBoxed");
            TryValid(streamDef + "sa.IntPrimitive = sb.IntPrimitive");
            TryValid(streamDef + "sa.IntPrimitive = sb.LongBoxed");

            TryInvalid(streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.IntBoxed = sa.BoolPrimitive");
            TryValid(streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.BoolBoxed = sa.BoolPrimitive");

            TryInvalid(streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.IntBoxed = sa.IntPrimitive and sa.TheString=sX.TheString");
            TryValid(streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.IntBoxed = sa.IntPrimitive and sa.TheString=sb.TheString");

            TryInvalid(streamDef + "sa.IntPrimitive = sb.IntPrimitive or sa.TheString=sX.TheString");
            TryValid(streamDef + "sa.IntPrimitive = sb.IntPrimitive or sb.IntBoxed = sa.IntPrimitive");

            // try constants
            TryValid(streamDef + "sa.IntPrimitive=5");
            TryValid(streamDef + "sa.TheString='4'");
            TryValid(streamDef + "sa.TheString=\"4\"");
            TryValid(streamDef + "sa.BoolPrimitive=false");
            TryValid(streamDef + "sa.LongPrimitive=-5L");
            TryValid(streamDef + "sa.DoubleBoxed=5.6d");
            TryValid(streamDef + "sa.FloatPrimitive=-5.6f");

            TryInvalid(streamDef + "sa.IntPrimitive='5'");
            TryInvalid(streamDef + "sa.TheString=5");
            TryInvalid(streamDef + "sa.BoolBoxed=f");
            TryInvalid(streamDef + "sa.IntPrimitive=x");
            TryValid(streamDef + "sa.IntPrimitive=5.5");

            // try addition and subtraction
            TryValid(streamDef + "sa.IntPrimitive=sa.IntBoxed + 5");
            TryValid(streamDef + "sa.IntPrimitive=2*sa.IntBoxed - sa.IntPrimitive/10 + 1");
            TryValid(streamDef + "sa.IntPrimitive=2*(sa.IntBoxed - sa.IntPrimitive)/(10 + 1)");
            TryInvalid(streamDef + "sa.IntPrimitive=2*(sa.IntBoxed");

            // try comparison
            TryValid(streamDef + "sa.IntPrimitive > sa.IntBoxed and sb.DoublePrimitive < sb.DoubleBoxed");
            TryValid(streamDef + "sa.IntPrimitive >= sa.IntBoxed and sa.DoublePrimitive <= sa.DoubleBoxed");
            TryValid(streamDef + "sa.IntPrimitive > (sa.IntBoxed + sb.DoublePrimitive)");
            TryInvalid(streamDef + "sa.IntPrimitive >= sa.String");

            // boolean testing valid as of 5.2
            //TryInvalid(streamDef + "sa.BoolBoxed >= sa.BoolPrimitive");

            // Try some nested
            TryValid(streamDef + "(sa.IntPrimitive=3) or (sa.IntBoxed=3 and sa.IntPrimitive=1)");
            TryValid(streamDef + "((sa.IntPrimitive>3) or (sa.IntBoxed<3)) and sa.BoolBoxed=false");
            TryValid(streamDef + "(sa.IntPrimitive<=3 and sa.IntPrimitive>=1) or (sa.BoolBoxed=false and sa.BoolPrimitive=true)");
            TryInvalid(streamDef + "sa.IntPrimitive=3 or (sa.IntBoxed=2");
            TryInvalid(streamDef + "sa.IntPrimitive=3 or sa.IntBoxed=2)");
            TryInvalid(streamDef + "sa.IntPrimitive=3 or ((sa.IntBoxed=2)");

            // Try some without stream name
            TryInvalid(streamDef + "IntPrimitive=3");
            TryValid(streamDefTwo + "IntPrimitive=3");

            // Try invalid outer join criteria
            String outerJoinDef = "select * from " +
                    typeof(SupportBean).FullName + "#length(3) as sa " +
                    "left outer join " +
                    typeof(SupportBean).FullName + "#length(3) as sb ";
            TryValid(outerJoinDef + "on sa.IntPrimitive = sb.IntBoxed");
            TryInvalid(outerJoinDef + "on sa.IntPrimitive = sb.XX");
            TryInvalid(outerJoinDef + "on sa.XX = sb.XX");
            TryInvalid(outerJoinDef + "on sa.XX = sb.IntBoxed");
            TryInvalid(outerJoinDef + "on sa.BoolBoxed = sb.IntBoxed");
            TryValid(outerJoinDef + "on sa.BoolPrimitive = sb.BoolBoxed");
            TryInvalid(outerJoinDef + "on sa.BoolPrimitive = sb.TheString");
            TryInvalid(outerJoinDef + "on sa.IntPrimitive <= sb.IntBoxed");
            TryInvalid(outerJoinDef + "on sa.IntPrimitive = sa.IntBoxed");
            TryInvalid(outerJoinDef + "on sb.IntPrimitive = sb.IntBoxed");
            TryValid(outerJoinDef + "on sb.IntPrimitive = sa.IntBoxed");
        }

        private void TryInvalid(String eplInvalidEPL)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(eplInvalidEPL);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // Expected exception
            }
        }

        private void TryValid(String invalidEPL)
        {
            _epService.EPAdministrator.CreateEPL(invalidEPL);
        }

        private String GetSyntaxExceptionEPL(String expression)
        {
            String exceptionText = null;
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException ex)
            {
                exceptionText = ex.Message;
                log.Debug(".getSyntaxExceptionEPL epl=" + expression, ex);
                // Expected exception
            }

            return exceptionText;
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
