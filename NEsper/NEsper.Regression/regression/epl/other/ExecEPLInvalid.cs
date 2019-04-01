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
    
            SupportMessageAssertUtil.TryInvalid(epService, "select count(TheString, TheString, TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'count(TheString,TheString,TheString)': The 'count' function expects at least 1 and up to 2 parameters");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select leaving(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'leaving(TheString)': The 'leaving' function expects no parameters");
        }
    
        private void RunAssertionInvalidSyntax(EPServiceProvider epService) {
            string exceptionText = GetSyntaxExceptionEPL(epService, "select * from *");
            Assert.AreEqual("Incorrect syntax near '*' at line 1 column 14, please check the from clause [select * from *]", exceptionText);
    
            exceptionText = GetSyntaxExceptionEPL(epService, "select * from SupportBean a where a.IntPrimitive between r.start and r.end");
            Assert.AreEqual("Incorrect syntax near 'start' (a reserved keyword) at line 1 column 59, please check the where clause [select * from SupportBean a where a.IntPrimitive between r.start and r.end]", exceptionText);
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from System.Object(1=2=3)",
                    "Failed to validate filter expression '1=2': Invalid use of equals, expecting left-hand side and right-hand side but received 3 expressions");
        }
    
        private void RunAssertionLongTypeConstant(EPServiceProvider epService) {
            string stmtText = "select 2512570244 as value from " + typeof(SupportBean).FullName;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
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
                    typeof(SupportMarketDataBean).FullName + "#length(3)" +
                    " where ";
    
            TryInvalid(epService, streamDef + "sa.IntPrimitive = sb.TheString");
            TryValid(epService, streamDef + "sa.IntPrimitive = sb.IntBoxed");
            TryValid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive");
            TryValid(epService, streamDef + "sa.IntPrimitive = sb.LongBoxed");
    
            TryInvalid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.IntBoxed = sa.BoolPrimitive");
            TryValid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.BoolBoxed = sa.BoolPrimitive");
    
            TryInvalid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.IntBoxed = sa.IntPrimitive and sa.TheString=sX.TheString");
            TryValid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive and sb.IntBoxed = sa.IntPrimitive and sa.TheString=sb.TheString");
    
            TryInvalid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive or sa.TheString=sX.TheString");
            TryValid(epService, streamDef + "sa.IntPrimitive = sb.IntPrimitive or sb.IntBoxed = sa.IntPrimitive");
    
            // try constants
            TryValid(epService, streamDef + "sa.IntPrimitive=5");
            TryValid(epService, streamDef + "sa.TheString='4'");
            TryValid(epService, streamDef + "sa.TheString=\"4\"");
            TryValid(epService, streamDef + "sa.BoolPrimitive=false");
            TryValid(epService, streamDef + "sa.LongPrimitive=-5L");
            TryValid(epService, streamDef + "sa.DoubleBoxed=5.6d");
            TryValid(epService, streamDef + "sa.FloatPrimitive=-5.6f");
    
            TryInvalid(epService, streamDef + "sa.IntPrimitive='5'");
            TryInvalid(epService, streamDef + "sa.TheString=5");
            TryInvalid(epService, streamDef + "sa.BoolBoxed=f");
            TryInvalid(epService, streamDef + "sa.IntPrimitive=x");
            TryValid(epService, streamDef + "sa.IntPrimitive=5.5");
    
            // try addition and subtraction
            TryValid(epService, streamDef + "sa.IntPrimitive=sa.IntBoxed + 5");
            TryValid(epService, streamDef + "sa.IntPrimitive=2*sa.IntBoxed - sa.IntPrimitive/10 + 1");
            TryValid(epService, streamDef + "sa.IntPrimitive=2*(sa.IntBoxed - sa.IntPrimitive)/(10 + 1)");
            TryInvalid(epService, streamDef + "sa.IntPrimitive=2*(sa.IntBoxed");
    
            // try comparison
            TryValid(epService, streamDef + "sa.IntPrimitive > sa.IntBoxed and sb.DoublePrimitive < sb.DoubleBoxed");
            TryValid(epService, streamDef + "sa.IntPrimitive >= sa.IntBoxed and sa.DoublePrimitive <= sa.DoubleBoxed");
            TryValid(epService, streamDef + "sa.IntPrimitive > (sa.IntBoxed + sb.DoublePrimitive)");
            TryInvalid(epService, streamDef + "sa.IntPrimitive >= sa.TheString");
            
            // boolean testing valid as of 5.2
            //TryInvalid(epService, streamDef + "sa.BoolBoxed >= sa.BoolPrimitive");

            // Try some nested
            TryValid(epService, streamDef + "(sa.IntPrimitive=3) or (sa.IntBoxed=3 and sa.IntPrimitive=1)");
            TryValid(epService, streamDef + "((sa.IntPrimitive>3) or (sa.IntBoxed<3)) and sa.BoolBoxed=false");
            TryValid(epService, streamDef + "(sa.IntPrimitive<=3 and sa.IntPrimitive>=1) or (sa.BoolBoxed=false and sa.BoolPrimitive=true)");
            TryInvalid(epService, streamDef + "sa.IntPrimitive=3 or (sa.IntBoxed=2");
            TryInvalid(epService, streamDef + "sa.IntPrimitive=3 or sa.IntBoxed=2)");
            TryInvalid(epService, streamDef + "sa.IntPrimitive=3 or ((sa.IntBoxed=2)");
    
            // Try some without stream name
            TryInvalid(epService, streamDef + "IntPrimitive=3");
            TryValid(epService, streamDefTwo + "IntPrimitive=3");
    
            // Try invalid outer join criteria
            string outerJoinDef = "select * from " +
                    typeof(SupportBean).FullName + "#length(3) as sa " +
                    "left outer join " +
                    typeof(SupportBean).FullName + "#length(3) as sb ";
            TryValid(epService, outerJoinDef + "on sa.IntPrimitive = sb.IntBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.IntPrimitive = sb.XX");
            TryInvalid(epService, outerJoinDef + "on sa.XX = sb.XX");
            TryInvalid(epService, outerJoinDef + "on sa.XX = sb.IntBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.BoolBoxed = sb.IntBoxed");
            TryValid(epService, outerJoinDef + "on sa.BoolPrimitive = sb.BoolBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.BoolPrimitive = sb.TheString");
            TryInvalid(epService, outerJoinDef + "on sa.IntPrimitive <= sb.IntBoxed");
            TryInvalid(epService, outerJoinDef + "on sa.IntPrimitive = sa.IntBoxed");
            TryInvalid(epService, outerJoinDef + "on sb.IntPrimitive = sb.IntBoxed");
            TryValid(epService, outerJoinDef + "on sb.IntPrimitive = sa.IntBoxed");
        }
    
        private void TryInvalid(EPServiceProvider epService, string eplInvalidEPL) {
            try {
                epService.EPAdministrator.CreateEPL(eplInvalidEPL);
                Assert.Fail();
            } catch (EPException) {
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
