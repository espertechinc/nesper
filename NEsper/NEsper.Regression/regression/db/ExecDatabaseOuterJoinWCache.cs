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
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseOuterJoinWCache : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionCatalog = "test";
            configDB.SetExpiryTimeCache(60, 120);

            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string stmtText = "select * from SupportBean as sb " +
                    "left outer join " +
                    "sql:MyDB ['select myint from mytesttable'] as t " +
                    "on sb.IntPrimitive = t.myint " +
                    "where myint is null";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
        }
    }
} // end of namespace
