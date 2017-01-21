///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
	public class TestFilterExpressionsOptimizable 
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();

	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportEvent", typeof(SupportTradeEvent));
	        config.AddEventType<SupportBean>();
	        config.AddEventType(typeof(SupportBean_IntAlphabetic));
	        config.AddEventType(typeof(SupportBean_StringAlphabetic));
	        config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;

	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	    }

        [TearDown]
	    public void TearDown() {
	        _listener = null;
	    }

        [Test]
	    public void TestOptimizablePerf()
        {
	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("libSplit", typeof(MyLib).FullName, "LibSplit", FilterOptimizable.ENABLED);
	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("libE1True", typeof(MyLib).FullName, "LibE1True", FilterOptimizable.ENABLED);

	        // create listeners
	        var count = 10;
	        var listeners = new SupportUpdateListener[count];
	        for (var i = 0; i < count; i++) {
	            listeners[i] = new SupportUpdateListener();
	        }

	        // func(...) = value
	        RunAssertionEquals("select * from SupportBean(libSplit(TheString) = !NUM!)", listeners);

	        // func(...) implied true
	        RunAssertionBoolean("select * from SupportBean(libE1True(TheString))");

	        // declared expression (...) = value
	        _epService.EPAdministrator.CreateEPL("create expression thesplit {TheString => libSplit(TheString)}");
	        RunAssertionEquals("select * from SupportBean(thesplit(*) = !NUM!)", listeners);

	        // declared expression (...) implied true
	        _epService.EPAdministrator.CreateEPL("create expression theE1Test {TheString => libE1True(TheString)}");
	        RunAssertionBoolean("select * from SupportBean(theE1Test(*))");

	        // typeof(e)
	        RunAssertionTypeOf();
	    }

        [Test]
	    public void TestOptimizableInspectFilter() {

	        string epl;

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("funcOne", typeof(MyLib).FullName, "LibSplit", FilterOptimizable.DISABLED);
	        epl = "select * from SupportBean(funcOne(TheString) = 0)";
	        AssertFilterSingle(epl, FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, FilterOperator.BOOLEAN_EXPRESSION);

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("funcOneWDefault", typeof(MyLib).FullName, "LibSplit");
	        epl = "select * from SupportBean(funcOneWDefault(TheString) = 0)";
	        AssertFilterSingle(epl, "funcOneWDefault(TheString)", FilterOperator.EQUAL);

	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("funcTwo", typeof(MyLib).FullName, "LibSplit",FilterOptimizable.ENABLED);
	        epl = "select * from SupportBean(funcTwo(TheString) = 0)";
	        AssertFilterSingle(epl, "funcTwo(TheString)", FilterOperator.EQUAL);

	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("libE1True", typeof(MyLib).FullName, "LibE1True",FilterOptimizable.ENABLED);
	        epl = "select * from SupportBean(libE1True(TheString))";
	        AssertFilterSingle(epl, "libE1True(TheString)", FilterOperator.EQUAL);

	        epl = "select * from SupportBean(funcTwo( TheString ) > 10)";
	        AssertFilterSingle(epl, "funcTwo(TheString)", FilterOperator.GREATER);

	        _epService.EPAdministrator.CreateEPL("create expression thesplit {TheString => funcOne(TheString)}");

	        epl = "select * from SupportBean(thesplit(*) = 0)";
	        AssertFilterSingle(epl, "thesplit(*)", FilterOperator.EQUAL);

	        epl = "select * from SupportBean(libE1True(TheString))";
	        AssertFilterSingle(epl, "libE1True(TheString)", FilterOperator.EQUAL);

	        epl = "select * from SupportBean(thesplit(*) > 10)";
	        AssertFilterSingle(epl, "thesplit(*)", FilterOperator.GREATER);

	        epl = "expression housenumber alias for {10} select * from SupportBean(IntPrimitive = housenumber)";
	        AssertFilterSingle(epl, "IntPrimitive", FilterOperator.EQUAL);

	        epl = "expression housenumber alias for {IntPrimitive*10} select * from SupportBean(IntPrimitive = housenumber)";
	        AssertFilterSingle(epl, ".boolean_expression", FilterOperator.BOOLEAN_EXPRESSION);

	        epl = "select * from SupportBean(typeof(e) = 'SupportBean') as e";
	        AssertFilterSingle(epl, "typeof(e)", FilterOperator.EQUAL);
	    }

        [Test]
	    public void TestPatternUDFFilterOptimizable()
        {
	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myCustomDecimalEquals", GetType().FullName, "MyCustomDecimalEquals");

	        var epl = "select * from pattern[a=SupportBean() -> b=SupportBean(myCustomDecimalEquals(a.DecimalBoxed, b.DecimalBoxed))]";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        var beanOne = new SupportBean("E1", 0);
            beanOne.DecimalBoxed = 13.0m;
	        _epService.EPRuntime.SendEvent(beanOne);

	        var beanTwo = new SupportBean("E2", 0);
	        beanTwo.DecimalBoxed = 13.0m;
	        _epService.EPRuntime.SendEvent(beanTwo);

	        Assert.IsTrue(_listener.IsInvoked);
	    }

        [Test]
	    public void TestOrToInRewrite()
	    {
	        // test 'or' rewrite
	        var filtersAB = new string[] {
	                "TheString = 'a' or TheString = 'b'",
	                "TheString = 'a' or 'b' = TheString",
	                "'a' = TheString or 'b' = TheString",
	                "'a' = TheString or TheString = 'b'",
	        };
	        foreach (var filter in filtersAB) {
	            var eplX = "select * from SupportBean(" + filter + ")";
	            AssertFilterSingle(eplX, "TheString", FilterOperator.IN_LIST_OF_VALUES);
	            _epService.EPAdministrator.CreateEPL(eplX).AddListener(_listener);

	            _epService.EPRuntime.SendEvent(new SupportBean("a", 0));
	            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	            _epService.EPRuntime.SendEvent(new SupportBean("b", 0));
	            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	            _epService.EPRuntime.SendEvent(new SupportBean("c", 0));
	            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	            _epService.EPAdministrator.DestroyAllStatements();
	        }

	        var epl = "select * from SupportBean(IntPrimitive = 1 and (TheString='a' or TheString='b'))";
	        AssertFilterTwo(epl, "IntPrimitive", FilterOperator.EQUAL, "TheString", FilterOperator.IN_LIST_OF_VALUES);
	    }

        [Test]
	    public void TestOrRewrite()
	    {
	        RunAssertionOrRewriteTwoOr();

	        RunAssertionOrRewriteThreeOr();

	        RunAssertionOrRewriteWithAnd();

	        RunAssertionOrRewriteThreeWithOverlap();

	        RunAssertionOrRewriteFourOr();

	        RunAssertionOrRewriteEightOr();

	        RunAssertionAndRewriteNotEquals();

	        RunAssertionAndRewriteInnerOr();

	        RunAssertionOrRewriteAndOrMulti();

	        RunAssertionBooleanExprSimple();

	        RunAssertionBooleanExprAnd();

	        RunAssertionContextPartitionedSegmented();

	        RunAssertionContextPartitionedHash();

	        RunAssertionContextPartitionedCategory();

	        RunAssertionSubquery();

	        RunAssertionHint();
	    }

	    private void RunAssertionHint() {
	        var epl = "@Hint('MAX_FILTER_WIDTH=0') select * from SupportBean_IntAlphabetic((b=1 or c=1) and (d=1 or e=1))";
	        AssertFilterSingle(epl, ".boolean_expression", FilterOperator.BOOLEAN_EXPRESSION);
	    }

	    private void RunAssertionSubquery() {
	        var epl = "select (select * from SupportBean_IntAlphabetic(a=1 or b=1).win:keepall()) as c0 from SupportBean";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var iaOne = IntEvent(1, 1);
	        _epService.EPRuntime.SendEvent(iaOne);
	        _epService.EPRuntime.SendEvent(new SupportBean());
	        Assert.AreEqual(iaOne, _listener.AssertOneGetNewAndReset().Get("c0"));

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionContextPartitionedCategory() {
	        _epService.EPAdministrator.CreateEPL("create context MyContext \n" +
	                "  group a=1 or b=1 as g1,\n" +
	                "  group c=1 as g1\n" +
	                "  from SupportBean_IntAlphabetic");
	        var epl = "context MyContext select * from SupportBean_IntAlphabetic(d=1 or e=1)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendAssertEvents(
	                new object[] {IntEvent(1, 0, 0, 0, 1), IntEvent(0, 1, 0, 1, 0), IntEvent(0, 0, 1, 1, 1)},
	                new object[] {IntEvent(0, 0, 0, 1, 0), IntEvent(1, 0, 0, 0, 0), IntEvent(0, 0, 1, 0, 0)}
	        );
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionContextPartitionedHash() {
	        _epService.EPAdministrator.CreateEPL("create context MyContext " +
	                "coalesce by consistent_hash_crc32(a) from SupportBean_IntAlphabetic(b=1) granularity 16 preallocate");
	        var epl = "context MyContext select * from SupportBean_IntAlphabetic(c=1 or d=1)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendAssertEvents(
	                new object[] {IntEvent(100, 1, 0, 1), IntEvent(100, 1, 1, 0)},
	                new object[] {IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0)}
	        );
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionContextPartitionedSegmented() {
	        _epService.EPAdministrator.CreateEPL("create context MyContext partition by a from SupportBean_IntAlphabetic(b=1 or c=1)");
	        var epl = "context MyContext select * from SupportBean_IntAlphabetic(d=1)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendAssertEvents(
	                new object[] {IntEvent(100, 1, 0, 1), IntEvent(100, 0, 1, 1)},
	                new object[] {IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0)}
	        );
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionBooleanExprAnd() {
	        var filters = new string[] {
	                "(a='a' or a like 'A%') and (b='b' or b like 'B%')",
	        };
	        foreach (var filter in filters) {
	            var epl = "select * from SupportBean_StringAlphabetic(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.EQUAL), GetBoolExprFilterItem()},
	                    new FilterItem[] {new FilterItem("b", FilterOperator.EQUAL), GetBoolExprFilterItem()},
	                    new FilterItem[] {GetBoolExprFilterItem()},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new object[] {StringEvent("a", "b"), StringEvent("A1", "b"), StringEvent("a", "B1"), StringEvent("A1", "B1")},
	                    new object[] {StringEvent("x", "b"), StringEvent("a", "x"), StringEvent("A1", "C"), StringEvent("C", "B1")}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionBooleanExprSimple() {
	        var filters = new string[] {
	                "a like 'a%' and (b='b' or c='c')",
	        };
	        foreach (var filter in filters) {
	            var epl = "select * from SupportBean_StringAlphabetic(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("b", FilterOperator.EQUAL), GetBoolExprFilterItem()},
	                    new FilterItem[] {new FilterItem("c", FilterOperator.EQUAL), GetBoolExprFilterItem()},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new object[] {StringEvent("a1", "b", null), StringEvent("a1", null, "c")},
	                    new object[] {StringEvent("x", "b", null), StringEvent("a1", null, null), StringEvent("a1", null, "x")}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionAndRewriteNotEquals() {
	        RunAssertionAndRewriteNotEqualsOr();

	        RunAssertionAndRewriteNotEqualsConsolidate();

	        RunAssertionAndRewriteNotEqualsWithOrConsolidateSecond();
	    }

	    private void RunAssertionAndRewriteNotEqualsWithOrConsolidateSecond() {
	        var filters = new string[] {
	                "a!=1 and a!=2 and ((a!=3 and a!=4) or (a!=5 and a!=6))",
	        };
	        foreach (var filter in filters) {
	            var epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), GetBoolExprFilterItem()},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), GetBoolExprFilterItem()},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new object[] {IntEvent(3), IntEvent(4), IntEvent(0)},
	                    new object[] {IntEvent(2), IntEvent(1)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionAndRewriteNotEqualsConsolidate() {
	        var filters = new string[] {
	                "a!=1 and a!=2 and (a!=3 or a!=4)",
	        };
	        foreach (var filter in filters) {
	            var epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("a", FilterOperator.NOT_EQUAL)},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("a", FilterOperator.NOT_EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new object[] {IntEvent(3), IntEvent(4), IntEvent(0)},
	                    new object[] {IntEvent(2), IntEvent(1)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionAndRewriteNotEqualsOr() {
	        var filters = new string[] {
	                "a!=1 and a!=2 and (b=1 or c=1)",
	        };
	        foreach (var filter in filters) {
	            var epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("b", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("c", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new object[] {IntEvent(3, 1, 0), IntEvent(3, 0, 1), IntEvent(0, 1, 0)},
	                    new object[] {IntEvent(2, 0, 0), IntEvent(1, 0, 0), IntEvent(3, 0, 0)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionAndRewriteInnerOr() {
	        var filtersAB = new string[] {
	                "TheString='a' and (IntPrimitive=1 or LongPrimitive=10)",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL), new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL), new FilterItem("LongPrimitive", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new SupportBean[] {MakeEvent("a", 1, 0), MakeEvent("a", 0, 10), MakeEvent("a", 1, 10)},
	                    new SupportBean[] {MakeEvent("x", 0, 0), MakeEvent("a", 2, 20), MakeEvent("x", 1, 10)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionOrRewriteAndOrMulti() {
	        var filtersAB = new string[] {
	                "a=1 and (b=1 or c=1) and (d=1 or e=1)",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL), new FilterItem("d", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.EQUAL), new FilterItem("c", FilterOperator.EQUAL), new FilterItem("d", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.EQUAL), new FilterItem("c", FilterOperator.EQUAL), new FilterItem("e", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL), new FilterItem("e", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new object[] {IntEvent(1, 1, 0, 1, 0), IntEvent(1, 0, 1, 0, 1), IntEvent(1, 1, 0, 0, 1), IntEvent(1, 0, 1, 1, 0)},
	                    new object[] {IntEvent(1, 0, 0, 1, 0), IntEvent(1, 0, 0, 1, 0), IntEvent(1, 1, 1, 0, 0), IntEvent(0, 1, 1, 1, 1)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionOrRewriteEightOr() {
	        var filtersAB = new string[] {
	                "TheString = 'a' or IntPrimitive=1 or LongPrimitive=10 or DoublePrimitive=100 or BoolPrimitive=true or " +
	                        "IntBoxed=2 or LongBoxed=20 or DoubleBoxed=200",
	                "LongBoxed=20 or TheString = 'a' or BoolPrimitive=true or IntBoxed=2 or LongPrimitive=10 or DoublePrimitive=100 or " +
	                        "IntPrimitive=1 or DoubleBoxed=200",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("LongPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("DoublePrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("BoolPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("IntBoxed", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("LongBoxed", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("DoubleBoxed", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new SupportBean[] {MakeEvent("a", 1, 10, 100, true, 2, 20, 200), MakeEvent("a", 0, 0, 0, true, 0, 0, 0),
	                            MakeEvent("a", 0, 0, 0, true, 0, 20, 0), MakeEvent("x", 0, 0, 100, false, 0, 0, 0),
	                            MakeEvent("x", 1, 0, 0, false, 0, 0, 200), MakeEvent("x", 0, 0, 0, false, 0, 0, 200),
	                    },
	                    new SupportBean[] {MakeEvent("x", 0, 0, 0, false, 0, 0, 0)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionOrRewriteFourOr() {
	        var filtersAB = new string[] {
	                "TheString = 'a' or IntPrimitive=1 or LongPrimitive=10 or DoublePrimitive=100",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("LongPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("DoublePrimitive", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new SupportBean[] {MakeEvent("a", 1, 10, 100), MakeEvent("x", 0, 0, 100), MakeEvent("x", 0, 10, 100), MakeEvent("a", 0, 0, 0)},
	                    new SupportBean[] {MakeEvent("x", 0, 0, 0)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionOrRewriteThreeWithOverlap() {
	        var filtersAB = new string[] {
	                "TheString = 'a' or TheString = 'b' or IntPrimitive=1",
	                "IntPrimitive = 1 or TheString = 'b' or TheString = 'a'",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new SupportBean[] {MakeEvent("a", 1), MakeEvent("b", 0), MakeEvent("x", 1)},
	                    new SupportBean[] {MakeEvent("x", 0)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionOrRewriteWithAnd() {
	        var filtersAB = new string[] {
	                "(TheString = 'a' and IntPrimitive = 1) or (TheString = 'b' and IntPrimitive = 2)",
	                "(IntPrimitive = 1 and TheString = 'a') or (IntPrimitive = 2 and TheString = 'b')",
	                "(TheString = 'b' and IntPrimitive = 2) or (TheString = 'a' and IntPrimitive = 1)",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL), new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL), new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new SupportBean[] {MakeEvent("a", 1), MakeEvent("b", 2)},
	                    new SupportBean[] {MakeEvent("x", 0), MakeEvent("a", 0), MakeEvent("a", 2), MakeEvent("b", 1)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void RunAssertionOrRewriteThreeOr() {
	        var filtersAB = new string[] {
	                "TheString = 'a' or IntPrimitive = 1 or LongPrimitive = 2",
	                "2 = LongPrimitive or 1 = IntPrimitive or TheString = 'a'"
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("LongPrimitive", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            SendAssertEvents(
	                    new SupportBean[] {MakeEvent("a", 0, 0), MakeEvent("b", 1, 0), MakeEvent("c", 0, 2), MakeEvent("c", 0, 2)},
	                    new SupportBean[] {MakeEvent("v", 0, 0), MakeEvent("c", 2, 1)}
	            );
	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

	    private void SendAssertEvents(object[] matches, object[] nonMatches) {
	        _listener.Reset();
	        foreach (var match in matches) {
	            _epService.EPRuntime.SendEvent(match);
	            Assert.AreSame(match, _listener.AssertOneGetNewAndReset().Underlying);
	        }
	        _listener.Reset();
	        foreach (var nonMatch in nonMatches) {
	            _epService.EPRuntime.SendEvent(nonMatch);
	            Assert.IsFalse(_listener.IsInvoked);
	        }
	    }

	    private void RunAssertionOrRewriteTwoOr() {
	        // test 'or' rewrite
	        var filtersAB = new string[] {
	                "TheString = 'a' or IntPrimitive = 1",
	                "TheString = 'a' or 1 = IntPrimitive",
	                "'a' = TheString or 1 = IntPrimitive",
	                "'a' = TheString or IntPrimitive = 1",
	        };
	        foreach (var filter in filtersAB) {
	            var epl = "select * from SupportBean(" + filter + ")";
	            var stmt = AssertFilterMulti(epl, new FilterItem[][] {
	                    new FilterItem[] {new FilterItem("IntPrimitive", FilterOperator.EQUAL)},
	                    new FilterItem[] {new FilterItem("TheString", FilterOperator.EQUAL)},
	            });
	            stmt.AddListener(_listener);

	            _epService.EPRuntime.SendEvent(new SupportBean("a", 0));
	            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	            _epService.EPRuntime.SendEvent(new SupportBean("b", 1));
	            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	            _epService.EPRuntime.SendEvent(new SupportBean("c", 0));
	            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	            _epService.EPAdministrator.DestroyAllStatements();
	        }
	    }

        [Test]
	    public void TestOrPerformance()
	    {
            foreach (var clazz in new Type[] { typeof(SupportBean) })
            {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            var listener = new SupportUpdateListener();
            for (var i = 0; i < 1000; i++)
            {
                var epl = "select * from SupportBean(TheString = '" + i + "' or IntPrimitive=" + i + ")";
                _epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
            }

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    Debug.WriteLine("Starting {0}", DateTime.Now.Print());

                    for (var i = 0; i < 10000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean("100", 1));
                        Assert.IsTrue(listener.IsInvoked);
                        listener.Reset();
                    }

                    Debug.WriteLine("Ending {0}", DateTime.Now.Print());
                });

            Debug.WriteLine("Delta={0}", delta + " msec");
            Assert.IsTrue(delta < 500);
	    }

        private EPStatement AssertFilterMulti(string epl, FilterItem[][] expected)
        {
            var statementSPI = (EPStatementSPI)_epService.EPAdministrator.CreateEPL(epl);
            var filterServiceSPI = (FilterServiceSPI)statementSPI.StatementContext.FilterService;
            var set = filterServiceSPI.Take(Collections.SingletonList(statementSPI.StatementId));
            var valueSet = set.Filters[0].FilterValueSet;
            var @params = valueSet.Parameters;

            var comparison = new Comparison<FilterItem>((o1, o2) =>
            {
                if (o1.Name == o2.Name)
                {
                    if (o1.Op > o2.Op)
                        return 1;
                    if (o1.Op < o2.Op)
                        return -1;
                    return 0;
                }

                return string.CompareOrdinal(o1.Name, o2.Name);
            });

            var found = new FilterItem[@params.Length][];
            for (var i = 0; i < found.Length; i++)
            {
                found[i] = new FilterItem[@params[i].Length];
                for (var j = 0; j < @params[i].Length; j++)
                {
                    found[i][j] = new FilterItem(@params[i][j].Lookupable.Expression, @params[i][j].FilterOperator);
                }
                Array.Sort(found[i], comparison);
            }

            for (var i = 0; i < expected.Length; i++)
            {
                Array.Sort(expected[i], comparison);
            }

            EPAssertionUtil.AssertEqualsAnyOrder(expected, found);
            filterServiceSPI.Apply(set);
            return statementSPI;
        }

	    private void RunAssertionEquals(string epl, SupportUpdateListener[] listeners) {

	        // test function returns lookup value and "equals"
	        for (var i = 0; i < listeners.Length; i++) {
	            var stmt = _epService.EPAdministrator.CreateEPL(epl.Replace("!NUM!", i.ToString(CultureInfo.InvariantCulture)));
	            stmt.AddListener(listeners[i]);
	        }

	        MyLib.ResetCountInvoked();

            var loops = 1000;
            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (var i = 0; i < loops; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean("E_" + i % listeners.Length, 0));
                        var listener = listeners[i % listeners.Length];
                        Assert.IsTrue(listener.GetAndClearIsInvoked());
                    }
                });

	        Assert.AreEqual(loops, MyLib.CountInvoked);

	        Log.Info("Equals delta=" + delta);
            Assert.That(delta, Is.LessThan(1000), "Delta is " + delta);
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionBoolean(string epl) {

	        // test function returns lookup value and "equals"
	        var count = 10;
	        for (var i = 0; i < count; i++) {
	            var stmt = _epService.EPAdministrator.CreateEPL(epl);
	            stmt.AddListener(_listener);
	        }

	        MyLib.ResetCountInvoked();

            var loops = 10000;
            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (var i = 0; i < loops; i++)
                    {
                        var key = "E_" + i % 100;

                        _epService.EPRuntime.SendEvent(new SupportBean(key, 0));

                        if (key.Equals("E_1"))
                        {
                            Assert.AreEqual(count, _listener.NewDataList.Count);
                            _listener.Reset();
                        }
                        else
                        {
                            Assert.IsFalse(_listener.IsInvoked);
                        }
                    }
                });

            // As noted in my analysis, the invocation count can actually be larger than the number of loops,
            // this occurs because of the way in which the weak references are handled in the caching layers
            // below.  Java soft-references behave much better, but we don't have access to them in the CLR.

            Assert.That(MyLib.CountInvoked, Is.GreaterThanOrEqualTo(loops),
               string.Format("MyLib.CountInvoked = {0}", MyLib.CountInvoked));

            Log.Info("Boolean delta=" + delta);
            Assert.That(delta, Is.LessThan(1000), "Delta is " + delta);
            _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void AssertFilterSingle(string epl, string expression, FilterOperator op) {
	        var statementSPI = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        if (((FilterServiceSPI) statementSPI.StatementContext.FilterService).IsSupportsTakeApply) {
	            var param = GetFilterSingle(statementSPI);
                Assert.AreEqual(op, param.FilterOperator, "failed for '" + epl + "'");
	            Assert.AreEqual(expression, param.Lookupable.Expression);
	        }
	    }

	    private void RunAssertionTypeOf() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportOverrideBase));
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportOverrideBase(typeof(e) = 'SupportOverrideBase') as e");
	        stmt.AddListener(_listener);
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportOverrideBase(""));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportOverrideOne("a", "b"));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        stmt.Dispose();
	    }

	    private void AssertFilterTwo(string epl, string expressionOne, FilterOperator opOne, string expressionTwo, FilterOperator opTwo) {
	        var statementSPI = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        if (((FilterServiceSPI) statementSPI.StatementContext.FilterService).IsSupportsTakeApply) {
	            var multi = GetFilterMulti(statementSPI);
	            Assert.AreEqual(2, multi.Length);
	            Assert.AreEqual(opOne, multi[0].FilterOperator);
	            Assert.AreEqual(expressionOne, multi[0].Lookupable.Expression);
	            Assert.AreEqual(opTwo, multi[1].FilterOperator);
	            Assert.AreEqual(expressionTwo, multi[1].Lookupable.Expression);
	        }
	    }

	    private FilterValueSetParam GetFilterSingle(EPStatementSPI statementSPI) {
	        var @params = GetFilterMulti(statementSPI);
	        Assert.AreEqual(1, @params.Length);
	        return @params[0];
	    }

	    private FilterValueSetParam[] GetFilterMulti(EPStatementSPI statementSPI) {
	        var filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
	        var set = filterServiceSPI.Take(Collections.SingletonList(statementSPI.StatementId));
	        Assert.AreEqual(1, set.Filters.Count);
	        FilterValueSet valueSet = set.Filters[0].FilterValueSet;
	        return valueSet.Parameters[0];
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive) {
	        return MakeEvent(theString, intPrimitive, 0L);
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
	        return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
	    }

	    private SupportBean_IntAlphabetic IntEvent(int a) {
	        return new SupportBean_IntAlphabetic(a);
	    }

	    private SupportBean_IntAlphabetic IntEvent(int a, int b) {
	        return new SupportBean_IntAlphabetic(a, b);
	    }

	    private SupportBean_IntAlphabetic IntEvent(int a, int b, int c, int d) {
	        return new SupportBean_IntAlphabetic(a, b, c, d);
	    }

	    private SupportBean_StringAlphabetic StringEvent(string a, string b) {
	        return new SupportBean_StringAlphabetic(a,b);
	    }

	    private SupportBean_StringAlphabetic StringEvent(string a, string b, string c) {
	        return new SupportBean_StringAlphabetic(a,b,c);
	    }

	    private SupportBean_IntAlphabetic IntEvent(int a, int b, int c) {
	        return new SupportBean_IntAlphabetic(a,b,c);
	    }

	    private SupportBean_IntAlphabetic IntEvent(int a, int b, int c, int d, int e) {
	        return new SupportBean_IntAlphabetic(a,b,c,d,e);
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive)
        {
	        var @event = new SupportBean(theString, intPrimitive);
	        @event.LongPrimitive = longPrimitive;
	        @event.DoublePrimitive = doublePrimitive;
	        return @event;
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive,
	                                  bool boolPrimitive, int intBoxed, long longBoxed, double doubleBoxed)
        {
	        var @event = new SupportBean(theString, intPrimitive);
	        @event.LongPrimitive = longPrimitive;
	        @event.DoublePrimitive = doublePrimitive;
	        @event.BoolPrimitive = boolPrimitive;
	        @event.LongBoxed = longBoxed;
	        @event.DoubleBoxed = doubleBoxed;
	        @event.IntBoxed = intBoxed;
	        return @event;
	    }

        public FilterItem GetBoolExprFilterItem()
        {
            return new FilterItem(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, FilterOperator.BOOLEAN_EXPRESSION);
        }

        public static bool MyCustomDecimalEquals(decimal first, decimal second) 
        {
	        return first.CompareTo(second) == 0;
	    }

	    public class MyLib
        {
	        private static int _countInvoked;

	        public static int LibSplit(string theString)
            {
	            var key = theString.Split('_');
	            _countInvoked++;
	            return int.Parse(key[1]);
	        }

	        public static bool LibE1True(string theString)
            {
	            _countInvoked++;
	            return theString.Equals("E_1");
	        }

	        public static int CountInvoked
	        {
	            get { return _countInvoked; }
	        }

	        public static void ResetCountInvoked()
            {
	            _countInvoked = 0;
	        }
	    }

	    public class FilterItem
        {
	        public FilterItem(string name, FilterOperator op)
            {
	            Name = name;
	            Op = op;
	        }

	        public string Name { get; private set; }

	        public FilterOperator Op { get; private set; }

	        public override string ToString()
            {
	            return "FilterItem new object[] {" +
	                    "name='" + Name + '\'' +
	                    ", op=" + Op +
	                    '}';
	        }

	        protected bool Equals(FilterItem other)
	        {
	            return string.Equals(Name, other.Name) && Op == other.Op;
	        }

	        public override bool Equals(object obj)
	        {
	            if (ReferenceEquals(null, obj))
	                return false;
	            if (ReferenceEquals(this, obj))
	                return true;
	            if (obj.GetType() != GetType())
	                return false;
	            return Equals((FilterItem) obj);
	        }

	        public override int GetHashCode()
	        {
	            unchecked
	            {
	                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (int) Op;
	            }
	        }
        }
	}
} // end of namespace
