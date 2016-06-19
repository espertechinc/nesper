///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestGroupByMaxMin 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _testListener;

        [SetUp]
	    public void SetUp()
	    {
	        _testListener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _testListener = null;
	    }

        [Test]
	    public void TestMinMaxView()
	    {
	        var viewExpr = "select irstream Symbol, " +
	                                  "min(all Volume) as minVol," +
	                                  "max(all Volume) as maxVol," +
	                                  "min(distinct Volume) as minDistVol," +
	                                  "max(distinct Volume) as maxDistVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_testListener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestMinMaxView_OM()
	    {
	        var model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.Create().SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
	            .Add("Symbol")
	            .Add(Expressions.Min("Volume"), "minVol")
	            .Add(Expressions.Max("Volume"), "maxVol")
	            .Add(Expressions.MinDistinct("Volume"), "minDistVol")
                .Add(Expressions.MaxDistinct("Volume"), "maxDistVol");

	        model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("win", "length", Expressions.Constant(3)));
	        model.WhereClause = Expressions.Or()
	                .Add(Expressions.Eq("Symbol", "DELL"))
	                .Add(Expressions.Eq("Symbol", "IBM"))
	                .Add(Expressions.Eq("Symbol", "GE")) ;
	        model.GroupByClause = GroupByClause.Create("Symbol");
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

	        var viewExpr = "select irstream Symbol, " +
	                                  "min(Volume) as minVol, " +
	                                  "max(Volume) as maxVol, " +
	                                  "min(distinct Volume) as minDistVol, " +
	                                  "max(distinct Volume) as maxDistVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
	                          "group by Symbol";
	        Assert.AreEqual(viewExpr, model.ToEPL());

	        var selectTestView = _epService.EPAdministrator.Create(model);
	        selectTestView.AddListener(_testListener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestMinMaxView_Compile()
	    {
	        var viewExpr = "select irstream Symbol, " +
	                                  "min(Volume) as minVol, " +
	                                  "max(Volume) as maxVol, " +
	                                  "min(distinct Volume) as minDistVol, " +
	                                  "max(distinct Volume) as maxDistVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
	                          "group by Symbol";
	        var model = _epService.EPAdministrator.CompileEPL(viewExpr);
	        Assert.AreEqual(viewExpr, model.ToEPL());

	        var selectTestView = _epService.EPAdministrator.Create(model);
	        selectTestView.AddListener(_testListener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestMinMaxJoin()
	    {
	        var viewExpr = "select irstream Symbol, " +
	                                  "min(Volume) as minVol," +
	                                  "max(Volume) as maxVol," +
	                                  "min(distinct Volume) as minDistVol," +
	                                  "max(distinct Volume) as maxDistVol" +
	                          " from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
	                                    typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
	                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
	                          "  and one.TheString = two.Symbol " +
	                          "group by Symbol";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestMinNoGroupHaving()
	    {
	        var stmtText = "select Symbol from " + typeof(SupportMarketDataBean).FullName + ".win:time(5 sec) " +
	                          "having Volume > min(Volume) * 1.3";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(stmtText);
	        selectTestView.AddListener(_testListener);

	        SendEvent("DELL", 100L);
	        SendEvent("DELL", 105L);
	        SendEvent("DELL", 100L);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("DELL", 131L);
	        Assert.AreEqual("DELL", _testListener.AssertOneGetNewAndReset().Get("Symbol"));

	        SendEvent("DELL", 132L);
	        Assert.AreEqual("DELL", _testListener.AssertOneGetNewAndReset().Get("Symbol"));

	        SendEvent("DELL", 129L);
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

        [Test]
	    public void TestMinNoGroupSelectHaving()
	    {
	        var stmtText = "select Symbol, min(Volume) as mymin from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                          "having Volume > min(Volume) * 1.3";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(stmtText);
	        selectTestView.AddListener(_testListener);

	        SendEvent("DELL", 100L);
	        SendEvent("DELL", 105L);
	        SendEvent("DELL", 100L);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("DELL", 131L);
	        var theEvent = _testListener.AssertOneGetNewAndReset();
	        Assert.AreEqual("DELL", theEvent.Get("Symbol"));
	        Assert.AreEqual(100L, theEvent.Get("mymin"));

	        SendEvent("DELL", 132L);
	        theEvent = _testListener.AssertOneGetNewAndReset();
	        Assert.AreEqual("DELL", theEvent.Get("Symbol"));
	        Assert.AreEqual(100L, theEvent.Get("mymin"));

	        SendEvent("DELL", 129L);
	        SendEvent("DELL", 125L);
	        SendEvent("DELL", 125L);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("DELL", 170L);
	        theEvent = _testListener.AssertOneGetNewAndReset();
	        Assert.AreEqual("DELL", theEvent.Get("Symbol"));
	        Assert.AreEqual(125L, theEvent.Get("mymin"));
	    }

	    private void RunAssertion(EPStatement selectTestView)
	    {
	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("minVol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("maxVol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("minDistVol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("maxDistVol"));

	        SendEvent(SYMBOL_DELL, 50L);
	        AssertEvents(SYMBOL_DELL, null, null, null, null,
	                SYMBOL_DELL, 50L, 50L, 50L, 50L
	                );

	        SendEvent(SYMBOL_DELL, 30L);
	        AssertEvents(SYMBOL_DELL, 50L, 50L, 50L, 50L,
	                SYMBOL_DELL, 30L, 50L, 30L, 50L
	                );

	        SendEvent(SYMBOL_DELL, 30L);
	        AssertEvents(SYMBOL_DELL, 30L, 50L, 30L, 50L,
	                SYMBOL_DELL, 30L, 50L, 30L, 50L
	                );

	        SendEvent(SYMBOL_DELL, 90L);
	        AssertEvents(SYMBOL_DELL, 30L, 50L, 30L, 50L,
	                SYMBOL_DELL, 30L, 90L, 30L, 90L
	                );

	        SendEvent(SYMBOL_DELL, 100L);
	        AssertEvents(SYMBOL_DELL, 30L, 90L, 30L, 90L,
	                SYMBOL_DELL, 30L, 100L, 30L, 100L
	                );

	        SendEvent(SYMBOL_IBM, 20L);
	        SendEvent(SYMBOL_IBM, 5L);
	        SendEvent(SYMBOL_IBM, 15L);
	        SendEvent(SYMBOL_IBM, 18L);
	        AssertEvents(SYMBOL_IBM, 5L, 20L, 5L, 20L,
	                SYMBOL_IBM, 5L, 18L, 5L, 18L
	                );

	        SendEvent(SYMBOL_IBM, null);
	        AssertEvents(SYMBOL_IBM, 5L, 18L, 5L, 18L,
	                SYMBOL_IBM, 15L, 18L, 15L, 18L
	                );

	        SendEvent(SYMBOL_IBM, null);
	        AssertEvents(SYMBOL_IBM, 15L, 18L, 15L, 18L,
	                SYMBOL_IBM, 18L, 18L, 18L, 18L
	                );

	        SendEvent(SYMBOL_IBM, null);
	        AssertEvents(SYMBOL_IBM, 18L, 18L, 18L, 18L,
	                SYMBOL_IBM, null, null, null, null
	                );
	    }

	    private void AssertEvents(string symbolOld, long? minVolOld, long? maxVolOld, long? minDistVolOld, long? maxDistVolOld,
	                              string symbolNew, long? minVolNew, long? maxVolNew, long? minDistVolNew, long? maxDistVolNew)
	    {
	        var oldData = _testListener.LastOldData;
	        var newData = _testListener.LastNewData;

	        Assert.AreEqual(1, oldData.Length);
	        Assert.AreEqual(1, newData.Length);

	        Assert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
	        Assert.AreEqual(minVolOld, oldData[0].Get("minVol"));
	        Assert.AreEqual(maxVolOld, oldData[0].Get("maxVol"));
	        Assert.AreEqual(minDistVolOld, oldData[0].Get("minDistVol"));
	        Assert.AreEqual(maxDistVolOld, oldData[0].Get("maxDistVol"));

	        Assert.AreEqual(symbolNew, newData[0].Get("Symbol"));
	        Assert.AreEqual(minVolNew, newData[0].Get("minVol"));
	        Assert.AreEqual(maxVolNew, newData[0].Get("maxVol"));
	        Assert.AreEqual(minDistVolNew, newData[0].Get("minDistVol"));
	        Assert.AreEqual(maxDistVolNew, newData[0].Get("maxDistVol"));

	        _testListener.Reset();
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

	    private void SendEvent(string symbol, long? volume)
	    {
	        var bean = new SupportMarketDataBean(symbol, 0, volume, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
