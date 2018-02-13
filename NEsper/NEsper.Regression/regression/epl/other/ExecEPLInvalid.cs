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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.fail;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLInvalid : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalidFuncParams(epService);
            RunAssertionInvalidSyntax(epService);
            RunAssertionLongTypeConstant(epService);
            RunAssertionDifferentJoins(epService);
        }
    
        private void RunAssertionInvalidFuncParams(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            SupportMessageAssertUtil.TryInvalid(epService, "select count(theString, theString, theString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'count(theString,theString,theString)': The 'count' function expects at least 1 and up to 2 parameters");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select leaving(theString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'leaving(theString)': The 'leaving' function expects no parameters");
        }
    
        private void RunAssertionInvalidSyntax(EPServiceProvider epService) {
            string exceptionText = GetSyntaxExceptionEPL(epService, "select * from *");
            Assert.AreEqual("Incorrect syntax near '*' at line 1 column 14, please check the from clause [select * from *]", exceptionText);
    
            exceptionText = GetSyntaxExceptionEPL(epService, "select * from SupportBean a where a.intPrimitive between r.start and r.end");
            Assert.AreEqual("Incorrect syntax near 'start' (a reserved keyword) at line 1 column 59, please check the where clause [select * from SupportBean a where a.intPrimitive between r.start and r.end]", exceptionText);
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from Java.lang.Object(1=2=3)",
                    "Failed to validate filter expression '1=2': Invalid use of equals, expecting left-hand side and right-hand side but received 3 expressions");
        }
    
        private void RunAssertionLongTypeConstant(EPServiceProvider epService) {
            string stmtText = "select 2512570244 as value from " + typeof(SupportBean).FullName;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(2512570244L, listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        private void RunAssertionDifferentJoins(EPServiceProvider epService) {
            try {
                epService.EPAdministrator.CreateEPL("select *");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: The from-clause is required but has not been specified [select *]", ex.Message);
            }
    
            string streamDef = "select * from " +
                    typeof(SupportBean).FullName + "#length(3) as sa," +
                    typeof(SupportBean).FullName + "#length(3) as sb" +
                    " where ";
    
            string streamDefTwo = "select * from " +
                    typeof(SupportBean).FullName + "#length(3)," +
                    typeof(SupportMarketDataBean).Name + "#length(3)" +
                    " where ";
    
            TryInvalid(epService, streamDef + "sa.intPrimitive = sb.theString");
            TryValid(epService, streamDef + "sa.intPrimitive = sb.intBoxed");
            TryValid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive");
            TryValid(epService, streamDef + "sa.intPrimitive = sb.longBoxed");
    
            TryInvalid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive and sb.intBoxed = sa.boolPrimitive");
            TryValid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive and sb.boolBoxed = sa.boolPrimitive");
    
            TryInvalid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive and sb.intBoxed = sa.intPrimitive and sa.theString=sX.theString");
            TryValid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive and sb.intBoxed = sa.intPrimitive and sa.theString=sb.theString");
    
            TryInvalid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive or sa.theString=sX.theString");
            TryValid(epService, streamDef + "sa.intPrimitive = sb.intPrimitive or sb.intBoxed = sa.intPrimitive");
    
            // try constants
            TryValid(epService, streamDef + "sa.intPrimitive=5");
            TryValid(epService, streamDef + "sa.theString='4'");
            TryValid(epService, streamDef + "sa.theString=\"4\"");
            TryValid(epService, streamDef + "sa.boolPrimitive=false");
            TryValid(epService, streamDef + "sa.longPrimitive=-5L");
            TryValid(epService, streamDef + "sa.doubleBoxed=5.6d");
            TryValid(epService, streamDef + "sa.floatPrimitive=-5.6f");
    
            TryInvalid(epService, streamDef + "sa.intPrimitive='5'");
            TryInvalid(epService, streamDef + "sa.theString=5");
            TryInvalid(epService, streamDef + "sa.boolBoxed=f");
            TryInvalid(epService, streamDef + "sa.intPrimitive=x");
            TryValid(epService, streamDef + "sa.intPrimitive=5.5");
    
            // try addition and subtraction
            TryValid(epService, streamDef + "sa.intPrimitive=sa.intBoxed + 5");
            TryValid(epService, streamDef + "sa.intPrimitive=2*sa.intBoxed - sa.intPrimitive/10 + 1");
            TryValid(epService, streamDef + "sa.intPrimitive=2*(sa.intBoxed - sa.intPrimitive)/(10 + 1)");
            TryInvalid(epService, streamDef + "sa.intPrimitive=2*(sa.intBoxed");
    
            // try comparison
            TryValid(epService, streamDef + "sa.intPrimitive > sa.intBoxed and sb.doublePrimitive < sb.doubleBoxed");
            TryValid(epService, streamDef + "sa.intPrimitive >= sa.intBoxed and sa.doublePrimitive <= sa.doubleBoxed");
            TryValid(epService, streamDef + "sa.intPrimitive > (sa.intBoxed + sb.doublePrimitive)");
            TryInvalid(epService, streamDef + "sa.intPrimitive >= sa.theString");
            TryInvalid(epService, streamDef + "sa.boolBoxed >= sa.boolPrimitive");
    
            // Try some nested
            TryValid(epService, streamDef + "(sa.intPrimitive=3) or (sa.intBoxed=3 and sa.intPrimitive=1)");
            TryValid(epService, streamDef + "((sa.intPrimitive>3) or (sa.intBoxed<3)) and sa.boolBoxed=false");
            TryValid(epService, streamDef + "(sa.intPrimitive<=3 and sa.intPrimitive>=1) or (sa.boolBoxed=false and sa.boolPrimitive=true)");
            TryInvalid(epService, streamDef + "sa.intPrimitive=3 or (sa.intBoxed=2");
            TryInvalid(epService, streamDef + "sa.intPrimitive=3 or sa.intBoxed=2)");
            TryInvalid(epService, streamDef + "sa.intPrimitive=3 or ((sa.intBoxed=2)");
    
            // Try some without stream name
            TryInvalid(epService, streamDef + "intPrimitive=3");
            TryValid(epService, streamDefTwo + "intPrimitive=3");
    
            // Try invalid outer join criteria
            string outerJoinDef = "select * from " +
                    typeof(SupportBean).FullName + "#length(3) as sa " +
                    "left outer join " +
                    typeof(SupportBean).FullName + "#length(3) as sb ";
            TryValid(epService, outerJoinDef + "on sa.intPrimitive = sb.intBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.intPrimitive = sb.XX");
            TryInvalid(epService, outerJoinDef + "on sa.XX = sb.XX");
            TryInvalid(epService, outerJoinDef + "on sa.XX = sb.intBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.boolBoxed = sb.intBoxed");
            TryValid(epService, outerJoinDef + "on sa.boolPrimitive = sb.boolBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.boolPrimitive = sb.theString");
            TryInvalid(epService, outerJoinDef + "on sa.intPrimitive <= sb.intBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.intPrimitive = sa.intBoxed");
            TryInvalid(epService, outerJoinDef + "on sb.intPrimitive = sb.intBoxed");
            TryValid(epService, outerJoinDef + "on sb.intPrimitive = sa.intBoxed");
        }
    
        private void TryInvalid(EPServiceProvider epService, string eplInvalidEPL) {
            try {
                epService.EPAdministrator.CreateEPL(eplInvalidEPL);
                Assert.Fail();
            } catch (EPException ex) {
                // Expected exception
            }
        }
    
        private void TryValid(EPServiceProvider epService, string invalidEPL) {
            epService.EPAdministrator.CreateEPL(invalidEPL);
        }
    
        private string GetSyntaxExceptionEPL(EPServiceProvider epService, string expression) {
            string exceptionText = null;
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPStatementSyntaxException ex) {
                exceptionText = ex.Message;
                Log.Debug(".getSyntaxExceptionEPL epl=" + expression, ex);
                // Expected exception
            }
    
            return exceptionText;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
