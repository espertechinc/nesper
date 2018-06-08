///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprLikeRegexp : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionRegexpFilterWithDanglingMetaCharacter(epService);
            RunAssertionLikeRegexStringAndNull(epService);
            RunAssertionLikeRegexEscapedChar(epService);
            RunAssertionLikeRegexStringAndNull_OM(epService);
            RunAssertionLikeRegexStringAndNull_Compile(epService);
            RunAssertionInvalidLikeRegEx(epService);
            RunAssertionLikeRegexNumericAndNull(epService);
        }
    
        private void RunAssertionRegexpFilterWithDanglingMetaCharacter(EPServiceProvider epService) {
            var stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString regexp \"*any*\"");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionLikeRegexStringAndNull(EPServiceProvider epService) {
            var caseExpr = "select p00 like p01 as r1, " +
                    " p00 like p01 escape \"!\" as r2," +
                    " p02 regexp p03 as r3 " +
                    " from " + typeof(SupportBean_S0).FullName;
    
            var stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunLikeRegexStringAndNull(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionLikeRegexEscapedChar(EPServiceProvider epService) {
            var caseExpr = "select p00 regexp '\\\\w*-ABC' as result from " + typeof(SupportBean_S0).FullName;
    
            var stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "TBT-ABC"));
            Assert.IsTrue((bool?) listener.AssertOneGetNewAndReset().Get("result"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "TBT-BC"));
            Assert.IsFalse((bool?) listener.AssertOneGetNewAndReset().Get("result"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionLikeRegexStringAndNull_OM(EPServiceProvider epService) {
            var stmtText = "select p00 like p01 as r1, " +
                    "p00 like p01 escape \"!\" as r2, " +
                    "p02 regexp p03 as r3 " +
                    "from " + typeof(SupportBean_S0).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.Like(Expressions.Property("p00"), Expressions.Property("p01")), "r1")
                .Add(Expressions.Like(Expressions.Property("p00"), Expressions.Property("p01"), Expressions.Constant("!")), "r2")
                .Add(Expressions.Regexp(Expressions.Property("p02"), Expressions.Property("p03")), "r3");

            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean_S0).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var stmt = epService.EPAdministrator.Create(model);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            RunLikeRegexStringAndNull(epService, testListener);
    
            stmt.Dispose();
    
            var epl = "select * from " + typeof(SupportBean).FullName + "(TheString not like \"foo%\")";
            var eps = epService.EPAdministrator.PrepareEPL(epl);
            var statement = epService.EPAdministrator.Create(eps);
            Assert.AreEqual(epl, statement.Text);
            statement.Dispose();
    
            epl = "select * from " + typeof(SupportBean).FullName + "(TheString not regexp \"foo\")";
            eps = epService.EPAdministrator.PrepareEPL(epl);
            statement = epService.EPAdministrator.Create(eps);
            Assert.AreEqual(epl, statement.Text);
            statement.Dispose();
        }
    
        private void RunAssertionLikeRegexStringAndNull_Compile(EPServiceProvider epService) {
            var stmtText = "select p00 like p01 as r1, " +
                    "p00 like p01 escape \"!\" as r2, " +
                    "p02 regexp p03 as r3 " +
                    "from " + typeof(SupportBean_S0).FullName;
    
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var stmt = epService.EPAdministrator.Create(model);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            RunLikeRegexStringAndNull(epService, testListener);
    
            stmt.Dispose();
        }
    
        private void RunLikeRegexStringAndNull(EPServiceProvider epService, SupportUpdateListener listener) {
            SendS0Event(epService, "a", "b", "c", "d");
            AssertReceived(listener, new[] {new object[] {"r1", false}, new object[] {"r2", false}, new object[] {"r3", false}});
    
            SendS0Event(epService, null, "b", null, "d");
            AssertReceived(listener, new[] {new object[] {"r1", null}, new object[] {"r2", null}, new object[] {"r3", null}});
    
            SendS0Event(epService, "a", null, "c", null);
            AssertReceived(listener, new[] {new object[] {"r1", null}, new object[] {"r2", null}, new object[] {"r3", null}});
    
            SendS0Event(epService, null, null, null, null);
            AssertReceived(listener, new[] {new object[] {"r1", null}, new object[] {"r2", null}, new object[] {"r3", null}});
    
            SendS0Event(epService, "abcdef", "%de_", "a", "[a-c]");
            AssertReceived(listener, new[] {new object[] {"r1", true}, new object[] {"r2", true}, new object[] {"r3", true}});
    
            SendS0Event(epService, "abcdef", "b%de_", "d", "[a-c]");
            AssertReceived(listener, new[] {new object[] {"r1", false}, new object[] {"r2", false}, new object[] {"r3", false}});
    
            SendS0Event(epService, "!adex", "!%de_", "", ".");
            AssertReceived(listener, new[] {new object[] {"r1", true}, new object[] {"r2", false}, new object[] {"r3", false}});
    
            SendS0Event(epService, "%dex", "!%de_", "a", ".");
            AssertReceived(listener, new[] {new object[] {"r1", false}, new object[] {"r2", true}, new object[] {"r3", true}});
        }
    
        private void RunAssertionInvalidLikeRegEx(EPServiceProvider epService) {
            TryInvalid(epService, "IntPrimitive like 'a' escape null");
            TryInvalid(epService, "IntPrimitive like BoolPrimitive");
            TryInvalid(epService, "BoolPrimitive like string");
            TryInvalid(epService, "string like string escape IntPrimitive");
    
            TryInvalid(epService, "IntPrimitive regexp doublePrimitve");
            TryInvalid(epService, "IntPrimitive regexp BoolPrimitive");
            TryInvalid(epService, "BoolPrimitive regexp string");
            TryInvalid(epService, "string regexp IntPrimitive");
        }
    
        private void RunAssertionLikeRegexNumericAndNull(EPServiceProvider epService) {
            var caseExpr = "select IntBoxed like '%01%' as r1, " +
                    " DoubleBoxed regexp '[0-9][0-9].[0-9][0-9]' as r2 " +
                    " from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var testListener = new SupportUpdateListener();
            selectTestCase.Events += testListener.Update;
    
            SendSupportBeanEvent(epService, 101, 1.1);
            AssertReceived(testListener, new[] {new object[] {"r1", true}, new object[] {"r2", false}});
    
            SendSupportBeanEvent(epService, 102, 11d);
            AssertReceived(testListener, new[] {new object[] {"r1", false}, new object[] {"r2", true}});
    
            SendSupportBeanEvent(epService, null, null);
            AssertReceived(testListener, new[] {new object[] {"r1", null}, new object[] {"r2", null}});
        }
    
        private void TryInvalid(EPServiceProvider epService, string expr) {
            try {
                var statement = "select " + expr + " from " + typeof(SupportBean).FullName;
                epService.EPAdministrator.CreateEPL(statement);
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    
        private void AssertReceived(SupportUpdateListener testListener, object[][] objects) {
            var theEvent = testListener.AssertOneGetNewAndReset();
            foreach (var @object in objects) {
                var key = (string) @object[0];
                var result = @object[1];
                Assert.AreEqual(result, theEvent.Get(key), "key=" + key + " result=" + result);
            }
        }
    
        private void SendS0Event(EPServiceProvider epService, string p00, string p01, string p02, string p03) {
            var bean = new SupportBean_S0(-1, p00, p01, p02, p03);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int? intBoxed, double? doubleBoxed) {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
