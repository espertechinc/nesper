///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextCategory : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionBooleanExprFilter(epService);
            RunAssertionContextPartitionSelection(epService);
            RunAssertionCategory(epService);
            RunAssertionSingleCategorySODAPrior(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionBooleanExprFilter(EPServiceProvider epService) {
            var eplCtx = "create context Ctx600a group by TheString like 'A%' as agroup, group by TheString like 'B%' as bgroup, group by TheString like 'C%' as cgroup from SupportBean";
            epService.EPAdministrator.CreateEPL(eplCtx);
            var eplSum = "context Ctx600a select context.label as c0, count(*) as c1 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(eplSum);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendAssertBooleanExprFilter(epService, listener, "B1", "bgroup", 1);
            SendAssertBooleanExprFilter(epService, listener, "A1", "agroup", 1);
            SendAssertBooleanExprFilter(epService, listener, "B171771", "bgroup", 2);
            SendAssertBooleanExprFilter(epService, listener, "A  x", "agroup", 2);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionContextPartitionSelection(EPServiceProvider epService) {
            var fields = "c0,c1,c2,c3".Split(',');
            epService.EPAdministrator.CreateEPL("create context MyCtx as group by IntPrimitive < -5 as grp1, group by IntPrimitive between -5 and +5 as grp2, group by IntPrimitive > 5 as grp3 from SupportBean");
            var stmt = epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, context.label as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean#keepall group by TheString");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -5));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", -100));
            epService.EPRuntime.SendEvent(new SupportBean("E3", -8));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 60));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new[] {
                new object[] {0, "grp1", "E3", -108},
                new object[] {1, "grp2", "E1", 3},
                new object[] {1, "grp2", "E2", -5},
                new object[] {2, "grp3", "E1", 60}
            });
    
            // test iterator targeted by context partition id
            var selectorById = new SupportSelectorById(Collections.SingletonSet(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selectorById), stmt.GetSafeEnumerator(selectorById), fields, new[] {new object[] {1, "grp2", "E1", 3}, new object[] {1, "grp2", "E2", -5}});
    
            // test iterator targeted for a given category
            var selector = new SupportSelectorCategory(new HashSet<string>(Collections.List("grp1", "grp3")));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new[] {new object[] {0, "grp1", "E3", -108}, new object[] {2, "grp3", "E1", 60}});
    
            // test iterator targeted for a given filtered category
            var filtered = new MySelectorFilteredCategory("grp1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new[] {new object[] {0, "grp1", "E3", -108}});
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorCategory((ISet<string>) null)).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorCategory(Collections.GetEmptySet<string>())).MoveNext());
    
            // test always-false filter - compare context partition info
            filtered = new MySelectorFilteredCategory(null);
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{"grp1", "grp2", "grp3"}, filtered.Categories);
    
            try {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented() {
                    ProcPartitionKeys = () => null
                });
                Assert.Fail();
            } catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorCategory] interfaces but received com."),
                    "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            // invalid filter spec
            epl = "create context ACtx group TheString is not null as cat1 from SupportBean(dummy = 1)";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // not a bool expression
            epl = "create context ACtx group IntPrimitive as grp1 from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: Filter expression not returning a boolean value: 'IntPrimitive' [");
    
            // validate statement not applicable filters
            epService.EPAdministrator.CreateEPL("create context ACtx group IntPrimitive < 10 as cat1 from SupportBean");
            epl = "context ACtx select * from SupportBean_S0";
            TryInvalid(epService, epl, "Error starting statement: Category context 'ACtx' requires that any of the events types that are listed in the category context also appear in any of the filter expressions of the statement [");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCategory(EPServiceProvider epService) {
            var filterSpi = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
            var ctx = "CategorizedContext";
            epService.EPAdministrator.CreateEPL("@Name('context') create context " + ctx + " " +
                    "group IntPrimitive < 10 as cat1, " +
                    "group IntPrimitive between 10 and 20 as cat2, " +
                    "group IntPrimitive > 20 as cat3 " +
                    "from SupportBean");
    
            var fields = "c0,c1,c2".Split(',');
            var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context CategorizedContext " +
                    "select context.name as c0, context.label as c1, sum(IntPrimitive) as c2 from SupportBean");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            Assert.AreEqual(3, filterSpi.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3, 0, 0, 0);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat1", 5});
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, new[] {new object[] {ctx, "cat1", 5}, new object[] {ctx, "cat2", null}, new object[] {ctx, "cat3", null}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat1", 9});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat2", 11});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 25));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat3", 25});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 25));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat3", 50});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat1", 12});
    
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, new[] {new object[] {ctx, "cat1", 12}, new object[] {ctx, "cat2", 11}, new object[] {ctx, "cat3", 50}});
    
            statement.Stop();
            Assert.AreEqual(0, filterSpi.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            var spi = (EPServiceProviderSPI) epService;
            Assert.AreEqual(1, spi.ContextManagementService.ContextCount);
            epService.EPAdministrator.GetStatement("context").Dispose();
            Assert.AreEqual(1, spi.ContextManagementService.ContextCount);
    
            statement.Dispose();
            Assert.AreEqual(0, spi.ContextManagementService.ContextCount);
        }
    
        private void RunAssertionSingleCategorySODAPrior(EPServiceProvider epService) {
            var ctx = "CategorizedContext";
            var eplCtx = "@Name('context') create context " + ctx + " as " +
                    "group IntPrimitive<10 as cat1 " +
                    "from SupportBean";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            var eplStmt = "context CategorizedContext select context.name as c0, context.label as c1, prior(1,IntPrimitive) as c2 from SupportBean";
            var statementOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            var listener = new SupportUpdateListener();
    
            RunAssertion(epService, listener, ctx, statementOne);
    
            // test SODA
            var modelContext = epService.EPAdministrator.CompileEPL(eplCtx);
            Assert.AreEqual(eplCtx, modelContext.ToEPL());
            var stmt = epService.EPAdministrator.Create(modelContext);
            Assert.AreEqual(eplCtx, stmt.Text);
    
            var modelStmt = epService.EPAdministrator.CompileEPL(eplStmt);
            Assert.AreEqual(eplStmt, modelStmt.ToEPL());
            var statementTwo = (EPStatementSPI) epService.EPAdministrator.Create(modelStmt);
            Assert.AreEqual(eplStmt, statementTwo.Text);
    
            RunAssertion(epService, listener, ctx, statementTwo);
        }
    
        private void RunAssertion(EPServiceProvider epService, SupportUpdateListener listener, string ctx, EPStatementSPI statement) {
            statement.Events += listener.Update;
    
            var fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "cat1", 5});
    
            epService.EPAdministrator.GetStatement("context").Dispose();
            var spi = (EPServiceProviderSPI) epService;
            Assert.AreEqual(1, spi.ContextManagementService.ContextCount);
    
            epService.EPAdministrator.DestroyAllStatements();
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
            Assert.AreEqual(0, spi.ContextManagementService.ContextCount);
        }
    
        private void SendAssertBooleanExprFilter(EPServiceProvider epService, SupportUpdateListener listener, string theString, string groupExpected, long countExpected) {
            var fields = "c0,c1".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean(theString, 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{groupExpected, countExpected});
        }

        internal class MySelectorFilteredCategory : ContextPartitionSelectorFiltered
        {
            private readonly string _matchCategory;
    
            private readonly List<object> _categories = new List<object>();
            private readonly LinkedHashSet<int?> _cpids = new LinkedHashSet<int?>();
    
            internal MySelectorFilteredCategory(string matchCategory) {
                _matchCategory = matchCategory;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                var id = (ContextPartitionIdentifierCategory) contextPartitionIdentifier;
                if (_matchCategory == null && _cpids.Contains(id.ContextPartitionId)) {
                    throw new EPRuntimeException("Already exists context id: " + id.ContextPartitionId);
                }
                _cpids.Add(id.ContextPartitionId);
                _categories.Add(id.Label);
                return _matchCategory != null && _matchCategory.Equals(id.Label);
            }

            internal object[] Categories => _categories.ToArray();
        }
    }
} // end of namespace
