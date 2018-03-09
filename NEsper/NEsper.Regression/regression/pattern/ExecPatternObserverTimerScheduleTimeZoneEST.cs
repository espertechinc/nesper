///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternObserverTimerScheduleTimeZoneEST : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.Expression.TimeZone = TimeZoneHelper.GetTimeZoneInfo("GMT-4:00");
        }
    
        public override void Run(EPServiceProvider epService) {
            EPServiceProviderIsolated iso = epService.GetEPServiceIsolated("E1");
            SendCurrentTime(iso, "2012-10-01T08:59:00.000GMT-04:00");
    
            string epl = "select * from pattern[timer:schedule(date: current_timestamp.WithTime(9, 0, 0, 0))]";
            var listener = new SupportUpdateListener();
            iso.EPAdministrator.CreateEPL(epl, null, null).Events += listener.Update;
    
            SendCurrentTime(iso, "2012-10-01T08:59:59.999GMT-4:00");
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            SendCurrentTime(iso, "2012-10-01T09:00:00.000GMT-4:00");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            SendCurrentTime(iso, "2012-10-03T09:00:00.000GMT-4:00");
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            epService.EPAdministrator.DestroyAllStatements();
            iso.Dispose();
        }
    
        private void SendCurrentTime(EPServiceProviderIsolated iso, string time) {
            iso.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSecWZone(time)));
        }
    }
    
    
} // end of namespace
