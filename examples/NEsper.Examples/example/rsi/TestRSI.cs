///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.Support;

using NUnit.Framework;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.RSI
{
	[TestFixture]
	public class TestRSI // : StockTickerRegressionConstants
	    : IDisposable
	{
		private const string SYMBOL = "GOOG";
		private const int PERIOD = 4;
	
	    private RSIStockTickerListener _stockListener;
	    private RSIListener _rsiListener;
	    private EPRuntime _runtime;
	    private EPStatement _factory;
	    private string _expressionText = null;
	
	    [SetUp]
	    public void SetUp()
	    {
	        var container = ContainerExtensions.CreateDefaultContainer()
                .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

	        var configuration = new Configuration(container);
            configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	        configuration.Common.AddEventType("StockTick", typeof(StockTick).FullName);
	        _runtime = EPRuntimeProvider.GetRuntime("TestStockTickerRSI", configuration);
	        _runtime.Initialize();
	
	        _stockListener = new RSIStockTickerListener(_runtime, PERIOD);
	        _expressionText = "every tick=StockTick(stockSymbol='" + SYMBOL + "')";
	        //_expressionText = "every tick1=StockTick(stockSymbol='GOOG') -> tick2=StockTick(stockSymbol='GOOG')";
	        _factory = _runtime.CompileDeploy(_expressionText).Statements[0];
	        _factory.Events += _stockListener.Update;
	
	        var rsiEvent = typeof(RSIEvent).FullName;
	        var viewExpr = "select * from " + rsiEvent + ".win:length(1)";
	        _rsiListener = new RSIListener();
	        _factory = _runtime.CompileDeploy(viewExpr).Statements[0];
	        _factory.Events += _rsiListener.Update;
			_rsiListener.Reset();
	    }
	    
	    [Test]	
		public void TestFlow() {
	
			// Bullish Stock, RSI rises beyond 70
			SendEvent(SYMBOL, 50);
			SendEvent(SYMBOL, 100);
			Assert.AreEqual(_rsiListener.AvgGain, Double.MinValue);
			Assert.AreEqual(_rsiListener.RS, Double.MinValue);
			Assert.AreEqual(_rsiListener.RSI, Double.MinValue);
			Assert.AreEqual(_rsiListener.RSICount, 0);
			SendEvent(SYMBOL, 75);
			SendEvent(SYMBOL, 100);
			SendEvent(SYMBOL, 150);
			// AvgLoss = 25 / (period = 4)
			Assert.AreEqual(_rsiListener.AvgLoss, -6.2);
			// AvgGain = (50 + 50 + 25) / (period = 4)
			Assert.AreEqual(_rsiListener.AvgGain, 31.2);
			// First RSI value when number of ticks = periods
			Assert.AreEqual(_rsiListener.RSICount, 1);
			SendEvent(SYMBOL, 125);
			// Add a couple of stock events
			// The trend is bullish, RSI goes beyond 70, overbought signal
			SendEvent(SYMBOL, 200);
			SendEvent(SYMBOL, 250);
			SendEvent(SYMBOL, 225);
			SendEvent(SYMBOL, 300);
	
		}

	    private void SendEvent(String symbol, double price)
	    {
	        var eventBean = new StockTick(symbol, price);
	        _runtime.EventService.SendEventBean(eventBean, eventBean.GetType().FullName);
	    }
	    
	    public void Dispose()
	    {
	    }
    }
}
