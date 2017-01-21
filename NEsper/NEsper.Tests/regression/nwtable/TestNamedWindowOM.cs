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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowOM 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerWindow;
        private SupportUpdateListener _listenerStmtOne;
        private SupportUpdateListener _listenerOnSelect;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listenerWindow = new SupportUpdateListener();
            _listenerStmtOne = new SupportUpdateListener();
            _listenerOnSelect = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerWindow = null;
            _listenerStmtOne = null;
            _listenerOnSelect = null;
        }
    
        [Test]
        public void TestCompile()
        {
            var fields = new string[] {"key", "value"};
            var stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            var modelCreate = _epService.EPAdministrator.CompileEPL(stmtTextCreate);
            var stmtCreate = _epService.EPAdministrator.Create(modelCreate);
            stmtCreate.AddListener(_listenerWindow);
            Assert.AreEqual("create window MyWindow.win:keepall() as select TheString as key, LongBoxed as value from com.espertech.esper.support.bean.SupportBean", modelCreate.ToEPL());
    
            var stmtTextOnSelect = "on " + typeof(SupportBean_B).FullName + " select mywin.* from MyWindow as mywin";
            var modelOnSelect = _epService.EPAdministrator.CompileEPL(stmtTextOnSelect);
            var stmtOnSelect = _epService.EPAdministrator.Create(modelOnSelect);
            stmtOnSelect.AddListener(_listenerOnSelect);
    
            var stmtTextInsert = "insert into MyWindow select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            var modelInsert = _epService.EPAdministrator.CompileEPL(stmtTextInsert);
            var stmtInsert = _epService.EPAdministrator.Create(modelInsert);
    
            var stmtTextSelectOne = "select irstream key, value*2 as value from MyWindow(key is not null)";
            var modelSelect = _epService.EPAdministrator.CompileEPL(stmtTextSelectOne);
            var stmtSelectOne = _epService.EPAdministrator.Create(modelSelect);
            stmtSelectOne.AddListener(_listenerStmtOne);
            Assert.AreEqual(stmtTextSelectOne, modelSelect.ToEPL());
    
            // send events
            SendSupportBean("E1", 10L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
    
            SendSupportBean("E2", 20L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol=s1.key";
            var modelDelete = _epService.EPAdministrator.CompileEPL(stmtTextDelete);
            _epService.EPAdministrator.Create(modelDelete);
            Assert.AreEqual("on com.espertech.esper.support.bean.SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.symbol=s1.key", modelDelete.ToEPL());
    
            // send delete event
            SendMarketBean("E1");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
    
            // send delete event again, none deleted now
            SendMarketBean("E1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            Assert.IsFalse(_listenerWindow.IsInvoked);
    
            // send delete event
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
    
            // trigger on-select on empty window
            Assert.IsFalse(_listenerOnSelect.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsFalse(_listenerOnSelect.IsInvoked);
    
            SendSupportBean("E3", 30L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 60L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30L});
    
            // trigger on-select on the filled window
            _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertProps(_listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30L});
    
            stmtSelectOne.Dispose();
            stmtInsert.Dispose();
            stmtCreate.Dispose();
        }
    
        [Test]
        public void TestOM()
        {
            var fields = new string[] {"key", "value"};
    
            // create window object model
            var model = new EPStatementObjectModel();
            model.CreateWindow = CreateWindowClause.Create("MyWindow").AddView("win", "keepall");
            model.SelectClause = SelectClause.Create()
                    .AddWithAsProvidedName("TheString", "key")
                    .AddWithAsProvidedName("LongBoxed", "value");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
    
            var stmtCreate = _epService.EPAdministrator.Create(model);
            stmtCreate.AddListener(_listenerWindow);
    
            var stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            Assert.AreEqual(stmtTextCreate, model.ToEPL());
    
            var stmtTextInsert = "insert into MyWindow select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            var modelInsert = _epService.EPAdministrator.CompileEPL(stmtTextInsert);
            var stmtInsert = _epService.EPAdministrator.Create(modelInsert);
    
            // Consumer statement object model
            model = new EPStatementObjectModel();
            Expression multi = Expressions.Multiply(Expressions.Property("value"), Expressions.Constant(2));
            model.SelectClause = SelectClause.Create().SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add("key")
                    .Add(multi, "value");
            model.FromClause = FromClause.Create(FilterStream.Create("MyWindow", Expressions.IsNotNull("value")));
    
            var stmtSelectOne = _epService.EPAdministrator.Create(model);
            stmtSelectOne.AddListener(_listenerStmtOne);
            var stmtTextSelectOne = "select irstream key, value*2 as value from MyWindow(value is not null)";
            Assert.AreEqual(stmtTextSelectOne, model.ToEPL());
    
            // send events
            SendSupportBean("E1", 10L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
    
            SendSupportBean("E2", 20L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
    
            // create delete stmt
            model = new EPStatementObjectModel();
            model.OnExpr = OnClause.CreateOnDelete("MyWindow", "s1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName, "s0"));
            model.WhereClause = Expressions.EqProperty("s0.symbol", "s1.key");
            _epService.EPAdministrator.Create(model);
            var stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol=s1.key";
            Assert.AreEqual(stmtTextDelete, model.ToEPL());
    
            // send delete event
            SendMarketBean("E1");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
    
            // send delete event again, none deleted now
            SendMarketBean("E1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            Assert.IsFalse(_listenerWindow.IsInvoked);
    
            // send delete event
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
    
            // On-select object model
            model = new EPStatementObjectModel();
            model.OnExpr = OnClause.CreateOnSelect("MyWindow", "s1");
            model.WhereClause = Expressions.EqProperty("s0.id", "s1.key");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean_B).FullName, "s0"));
            model.SelectClause = SelectClause.CreateStreamWildcard("s1");
            var statement = _epService.EPAdministrator.Create(model);
            statement.AddListener(_listenerOnSelect);
            var stmtTextOnSelect = "on " + typeof(SupportBean_B).FullName + " as s0 select s1.* from MyWindow as s1 where s0.id=s1.key";
            Assert.AreEqual(stmtTextOnSelect, model.ToEPL());
    
            // send some more events
            SendSupportBean("E3", 30L);
            SendSupportBean("E4", 40L);
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsFalse(_listenerOnSelect.IsInvoked);
    
            // trigger on-select
            _epService.EPRuntime.SendEvent(new SupportBean_B("E3"));
            EPAssertionUtil.AssertProps(_listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30L});
    
            stmtSelectOne.Dispose();
            stmtInsert.Dispose();
            stmtCreate.Dispose();
        }
    
        [Test]
        public void TestOMCreateTableSyntax()
        {
            var expected = "create window MyWindow.win:keepall() as (a1 string, a2 double, a3 int)";
    
            // create window object model
            var model = new EPStatementObjectModel();
            var clause = CreateWindowClause.Create("MyWindow").AddView("win", "keepall");
            clause.AddColumn(new SchemaColumnDesc("a1", "string", false, false));
            clause.AddColumn(new SchemaColumnDesc("a2", "double", false, false));
            clause.AddColumn(new SchemaColumnDesc("a3", "int", false, false));
            model.CreateWindow = clause;
            Assert.AreEqual(expected, model.ToEPL());
    
            var stmtCreate = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(expected, stmtCreate.Text);
        }
    
        private SupportBean SendSupportBean(string theString, long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongBoxed = longBoxed;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendMarketBean(string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0l, "");
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
