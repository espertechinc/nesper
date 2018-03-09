///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinNoTableName : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            var updateListener = new SupportUpdateListener();
            var joinStatement = "select * from " +
                                typeof(SupportMarketDataBean).FullName + "#length(3)," +
                                typeof(SupportBean).FullName + "#length(3)" +
                                " where symbol=TheString and volume=LongBoxed";

            var joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;

            var setOne = new object[5];
            var setTwo = new object[5];

            for (var i = 0; i < setOne.Length; i++)
            {
                setOne[i] = new SupportMarketDataBean("IBM", 0, i, "");

                var theEvent = new SupportBean();
                theEvent.TheString = "IBM";
                theEvent.LongBoxed = i;
                setTwo[i] = theEvent;
            }

            SendEvent(epService, setOne[0]);
            SendEvent(epService, setTwo[0]);
            Assert.IsNotNull(updateListener.LastNewData);
        }

        private void SendEvent(EPServiceProvider epService, object theEvent)
        {
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace