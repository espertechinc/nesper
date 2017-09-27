///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
	public class TestNamedWindowOnDelete
    {
	    private EPServiceProviderSPI _epService;
	    private SupportUpdateListener _listenerWindow;
	    private SupportUpdateListener _listenerWindowTwo;
	    private SupportUpdateListener _listenerSelect;
	    private SupportUpdateListener _listenerDelete;

        [SetUp]
	    public void SetUp() {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.Logging.IsEnableQueryPlan = true;
	        _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);
	        }
	        _listenerWindow = new SupportUpdateListener();
	        _listenerWindowTwo = new SupportUpdateListener();
	        _listenerSelect = new SupportUpdateListener();
	        _listenerDelete = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listenerDelete = null;
	        _listenerSelect = null;
	        _listenerWindow = null;
	        _listenerDelete = null;
	    }

        [Test]
	    public void TestFirstUnique() {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));

	        var fields = new string[] {"theString","intPrimitive"};
	        var stmtTextCreateOne = "create window MyWindowOne#firstunique(theString) as select * from SupportBean";
	        var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
	        _epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean");
	        var stmtDelete = _epService.EPAdministrator.CreateEPL("on SupportBean_A a delete from MyWindowOne where theString=a.id");
	        stmtDelete.AddListener(_listenerDelete);

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("A", 2));

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A"));
	        EPAssertionUtil.AssertProps(_listenerDelete.AssertOneGetNewAndReset(), fields, new object[] {"A", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 3));
	        EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] {new object[] {"A", 3}});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A"));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
	    }

        [Test]
	    public void TestStaggeredNamedWindow() {
	        foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
	            RunAssertionStaggered(rep);
	        }
	    }

	    private void RunAssertionStaggered(EventRepresentationChoice outputType) {

	        var fieldsOne = new string[] {"a1", "b1"};
	        var fieldsTwo = new string[] {"a2", "b2"};

	        // create window one
	        var stmtTextCreateOne = outputType.GetAnnotationText() + " create window MyWindowOne#keepall as select theString as a1, intPrimitive as b1 from " + typeof(SupportBean).FullName;
	        var stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
	        stmtCreateOne.AddListener(_listenerWindow);
	        Assert.AreEqual(0, GetCount("MyWindowOne"));
	        Assert.IsTrue(outputType.MatchesClass(stmtCreateOne.EventType.UnderlyingType));

	        // create window two
	        var stmtTextCreateTwo = outputType.GetAnnotationText() + " create window MyWindowTwo#keepall as select theString as a2, intPrimitive as b2 from " + typeof(SupportBean).FullName;
	        var stmtCreateTwo = _epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
	        stmtCreateTwo.AddListener(_listenerWindowTwo);
	        Assert.AreEqual(0, GetCount("MyWindowTwo"));
	        Assert.IsTrue(outputType.MatchesClass(stmtCreateTwo.EventType.UnderlyingType));

	        // create delete stmt
	        var stmtTextDelete = "on MyWindowOne delete from MyWindowTwo where a1 = a2";
	        var stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
	        stmtDelete.AddListener(_listenerDelete);
	        Assert.AreEqual(StatementType.ON_DELETE, ((EPStatementSPI) stmtDelete).StatementMetadata.StatementType);

	        // create insert into
            var stmtTextInsert = "insert into MyWindowOne select theString as a1, intPrimitive as b1 from " + typeof(SupportBean).FullName + "(intPrimitive > 0)";
	        _epService.EPAdministrator.CreateEPL(stmtTextInsert);
            stmtTextInsert = "insert into MyWindowTwo select theString as a2, intPrimitive as b2 from " + typeof(SupportBean).FullName + "(intPrimitive < 0)";
	        _epService.EPAdministrator.CreateEPL(stmtTextInsert);

	        SendSupportBean("E1", -10);
	        EPAssertionUtil.AssertProps(_listenerWindowTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"E1", -10});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsTwo, new object[][] { new object[] { "E1", -10 } });
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        Assert.AreEqual(1, GetCount("MyWindowTwo"));

	        SendSupportBean("E2", 5);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E2", 5});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsOne, new object[][] { new object[] { "E2", 5 } });
	        Assert.IsFalse(_listenerWindowTwo.IsInvoked);
	        Assert.AreEqual(1, GetCount("MyWindowOne"));

	        SendSupportBean("E3", -1);
	        EPAssertionUtil.AssertProps(_listenerWindowTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {"E3", -1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsTwo, new object[][] { new object[] { "E1", -10 }, new object[] { "E3", -1 } });
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        Assert.AreEqual(2, GetCount("MyWindowTwo"));

	        SendSupportBean("E3", 1);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E3", 1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsOne, new object[][] { new object[] { "E2", 5 }, new object[] { "E3", 1 } });
	        EPAssertionUtil.AssertProps(_listenerWindowTwo.AssertOneGetOldAndReset(), fieldsTwo, new object[] {"E3", -1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsTwo, new object[][] { new object[] { "E1", -10 } });
	        Assert.AreEqual(2, GetCount("MyWindowOne"));
	        Assert.AreEqual(1, GetCount("MyWindowTwo"));

	        stmtDelete.Dispose();
	        stmtCreateOne.Dispose();
	        stmtCreateTwo.Dispose();
	        _listenerDelete.Reset();
	        _listenerSelect.Reset();
	        _listenerWindow.Reset();
	        _listenerWindowTwo.Reset();
	        _epService.EPAdministrator.Configuration.RemoveEventType("MyWindowOne", true);
	        _epService.EPAdministrator.Configuration.RemoveEventType("MyWindowTwo", true);
	    }

        [Test]
	    public void TestCoercionKeyMultiPropIndexes() {
	        // create window
	        var stmtTextCreate = "create window MyWindow#keepall as select " +
                                    "theString, intPrimitive, intBoxed, doublePrimitive, doubleBoxed from " + typeof(SupportBean).FullName;
	        var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
	        stmtCreate.AddListener(_listenerWindow);

	        IList<EPStatement> deleteStatements = new List<EPStatement>();
            var stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='DB') as s0 delete from MyWindow as win where win.intPrimitive = s0.doubleBoxed";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='DP') as s0 delete from MyWindow as win where win.intPrimitive = s0.doublePrimitive";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='IB') as s0 delete from MyWindow where MyWindow.intPrimitive = s0.intBoxed";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='IPDP') as s0 delete from MyWindow as win where win.intPrimitive = s0.intPrimitive and win.doublePrimitive = s0.doublePrimitive";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='IPDP2') as s0 delete from MyWindow as win where win.doublePrimitive = s0.doublePrimitive and win.intPrimitive = s0.intPrimitive";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='IPDPIB') as s0 delete from MyWindow as win where win.doublePrimitive = s0.doublePrimitive and win.intPrimitive = s0.intPrimitive and win.intBoxed = s0.intBoxed";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='CAST') as s0 delete from MyWindow as win where win.intBoxed = s0.intPrimitive and win.doublePrimitive = s0.doubleBoxed and win.intPrimitive = s0.intBoxed";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        // create insert into
	        var stmtTextInsertOne = "insert into MyWindow select theString, intPrimitive, intBoxed, doublePrimitive, doubleBoxed "
                                       + "from " + typeof(SupportBean).FullName + "(theString like 'E%')";
	        _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);

	        SendSupportBean("E1", 1, 10, 100d, 1000d);
	        SendSupportBean("E2", 2, 20, 200d, 2000d);
	        SendSupportBean("E3", 3, 30, 300d, 3000d);
	        SendSupportBean("E4", 4, 40, 400d, 4000d);
	        _listenerWindow.Reset();

	        var fields = new string[] {"theString"};
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });

	        SendSupportBean("DB", 0, 0, 0d, null);
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        SendSupportBean("DB", 0, 0, 0d, 3d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E3"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E4" } });

	        SendSupportBean("DP", 0, 0, 5d, null);
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        SendSupportBean("DP", 0, 0, 4d, null);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E4"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1" }, new object[] { "E2" } });

	        SendSupportBean("IB", 0, -1, 0d, null);
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        SendSupportBean("IB", 0, 1, 0d, null);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E1"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2" } });

	        SendSupportBean("E5", 5, 50, 500d, 5000d);
	        SendSupportBean("E6", 6, 60, 600d, 6000d);
	        SendSupportBean("E7", 7, 70, 700d, 7000d);
	        _listenerWindow.Reset();

	        SendSupportBean("IPDP", 5, 0, 500d, null);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E5"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2" }, new object[] { "E6" }, new object[] { "E7" } });

	        SendSupportBean("IPDP2", 6, 0, 600d, null);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E6"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2" }, new object[] { "E7" } });

	        SendSupportBean("IPDPIB", 7, 70, 0d, null);
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        SendSupportBean("IPDPIB", 7, 70, 700d, null);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E7"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2" } });

	        SendSupportBean("E8", 8, 80, 800d, 8000d);
	        _listenerWindow.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2" }, new object[] { "E8" } });

	        SendSupportBean("CAST", 80, 8, 0, 800d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E8"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2" } });

	        foreach (var stmt in deleteStatements) {
	            stmt.Dispose();
	        }
	        deleteStatements.Clear();

	        // late delete on a filled window
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(theString='LAST') as s0 delete from MyWindow as win where win.intPrimitive = s0.intPrimitive and win.doublePrimitive = s0.doublePrimitive";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        SendSupportBean("LAST", 2, 20, 200, 2000d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E2"});
	        EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);

	        foreach (var stmt in deleteStatements) {
	            stmt.Dispose();
	        }

	        // test single-two-field index reuse
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.CreateEPL("create window WinOne#keepall as SupportBean");
	        _epService.EPAdministrator.CreateEPL("on SupportBean_ST0 select * from WinOne where theString = key0");
	        Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("WinOne").Length);

	        _epService.EPAdministrator.CreateEPL("on SupportBean_ST0 select * from WinOne where theString = key0 and intPrimitive = p00");
	        Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("WinOne").Length);
	    }

        [Test]
	    public void TestCoercionRangeMultiPropIndexes()
        {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanTwo", typeof(SupportBeanTwo));

	        // create window
	        var stmtTextCreate = "create window MyWindow#keepall as select " +
	                                "theString, intPrimitive, intBoxed, doublePrimitive, doubleBoxed from SupportBean";
	        var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
	        stmtCreate.AddListener(_listenerWindow);
	        var stmtText = "insert into MyWindow select theString, intPrimitive, intBoxed, doublePrimitive, doubleBoxed from SupportBean";
	        _epService.EPAdministrator.CreateEPL(stmtText);
	        var fields = new string[] {"theString"};

	        SendSupportBean("E1", 1, 10, 100d, 1000d);
	        SendSupportBean("E2", 2, 20, 200d, 2000d);
	        SendSupportBean("E3", 3, 30, 3d, 30d);
	        SendSupportBean("E4", 4, 40, 4d, 40d);
	        SendSupportBean("E5", 5, 50, 500d, 5000d);
	        SendSupportBean("E6", 6, 60, 600d, 6000d);
	        _listenerWindow.Reset();

	        IList<EPStatement> deleteStatements = new List<EPStatement>();
	        var stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindow as win where win.intPrimitive between s2.doublePrimitiveTwo and s2.doubleBoxedTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", 0, 0, 0d, null);
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        SendSupportBeanTwo("T", 0, 0, -1d, 1d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E1"});

	        stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindow as win where win.intPrimitive between s2.intPrimitiveTwo and s2.intBoxedTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", -2, 2, 0d, 0d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E2"});

	        stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindow as win " +
	                         "where win.intPrimitive between s2.intPrimitiveTwo and s2.intBoxedTwo and win.doublePrimitive between s2.intPrimitiveTwo and s2.intBoxedTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", -3, 3, -3d, 3d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E3"});

	        stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindow as win " +
	                         "where win.doublePrimitive between s2.intPrimitiveTwo and s2.intPrimitiveTwo and win.intPrimitive between s2.intPrimitiveTwo and s2.intPrimitiveTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", -4, 4, -4, 4d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E4"});

	        stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindow as win where win.intPrimitive <= doublePrimitiveTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", 0, 0, 5, 1d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E5"});

	        stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindow as win where win.intPrimitive not between s2.intPrimitiveTwo and s2.intBoxedTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", 100, 200, 0, 0d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E6"});

	        // delete
	        foreach (var stmt in deleteStatements) {
	            stmt.Dispose();
	        }
	        deleteStatements.Clear();
	        Assert.AreEqual(0, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
	    }

        [Test]
	    public void TestCoercionKeyAndRangeMultiPropIndexes() {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanTwo", typeof(SupportBeanTwo));

	        // create window
	        var stmtTextCreate = "create window MyWindow#keepall as select " +
	                                "theString, intPrimitive, intBoxed, doublePrimitive, doubleBoxed from SupportBean";
	        var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
	        stmtCreate.AddListener(_listenerWindow);
	        var stmtText = "insert into MyWindow select theString, intPrimitive, intBoxed, doublePrimitive, doubleBoxed from SupportBean";
	        _epService.EPAdministrator.CreateEPL(stmtText);
	        var fields = new string[] {"theString"};

	        SendSupportBean("E1", 1, 10, 100d, 1000d);
	        SendSupportBean("E2", 2, 20, 200d, 2000d);
	        SendSupportBean("E3", 3, 30, 300d, 3000d);
	        SendSupportBean("E4", 4, 40, 400d, 4000d);
	        _listenerWindow.Reset();

	        IList<EPStatement> deleteStatements = new List<EPStatement>();
	        var stmtTextDelete = "on SupportBeanTwo delete from MyWindow where theString = stringTwo and intPrimitive between doublePrimitiveTwo and doubleBoxedTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("T", 0, 0, 1d, 200d);
	        Assert.IsFalse(_listenerWindow.IsInvoked);
	        SendSupportBeanTwo("E1", 0, 0, 1d, 200d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E1"});

	        stmtTextDelete = "on SupportBeanTwo delete from MyWindow where theString = stringTwo and intPrimitive = intPrimitiveTwo and intBoxed between doublePrimitiveTwo and doubleBoxedTwo";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("E2", 2, 0, 19d, 21d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E2"});

	        stmtTextDelete = "on SupportBeanTwo delete from MyWindow where intBoxed between doubleBoxedTwo and doublePrimitiveTwo and intPrimitive = intPrimitiveTwo and theString = stringTwo ";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(2, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("E3", 3, 0, 29d, 34d);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E3"});

	        stmtTextDelete = "on SupportBeanTwo delete from MyWindow where intBoxed between intBoxedTwo and intBoxedTwo and intPrimitive = intPrimitiveTwo and theString = stringTwo ";
	        deleteStatements.Add(_epService.EPAdministrator.CreateEPL(stmtTextDelete));
	        Assert.AreEqual(3, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

	        SendSupportBeanTwo("E4", 4, 40, 0d, null);
	        EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[] {"E4"});

	        // delete
	        foreach (var stmt in deleteStatements) {
	            stmt.Dispose();
	        }
	        deleteStatements.Clear();
	        Assert.AreEqual(0, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
	    }

	    private SupportBean SendSupportBean(string theString, int intPrimitive, int? intBoxed, double doublePrimitive, double? doubleBoxed)
        {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        bean.IntBoxed = intBoxed;
	        bean.DoublePrimitive = doublePrimitive;
	        bean.DoubleBoxed = doubleBoxed;
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private SupportBeanTwo SendSupportBeanTwo(string theString, int intPrimitive, int? intBoxed, double doublePrimitive, double? doubleBoxed)
        {
	        var bean = new SupportBeanTwo();
	        bean.StringTwo = theString;
	        bean.IntPrimitiveTwo = intPrimitive;
	        bean.IntBoxedTwo = intBoxed;
	        bean.DoublePrimitiveTwo = doublePrimitive;
	        bean.DoubleBoxedTwo = doubleBoxed;
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private long GetCount(string windowName)
        {
	        var processor = _epService.NamedWindowMgmtService.GetProcessor(windowName);
	        return processor.GetProcessorInstance(null).CountDataWindow;
	    }
	}
} // end of namespace
