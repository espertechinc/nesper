///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowIndexAddedValType : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
    
            var revType = new ConfigurationRevisionEventType();
            revType.AddNameBaseEventType("SupportBean_S0");
            revType.AddNameDeltaEventType("SupportBean_S1");
            revType.KeyPropertyNames = new string[]{"Id"};
            revType.PropertyRevision = PropertyRevisionEnum.MERGE_EXISTS;
            configuration.AddRevisionEventType("RevType", revType);
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtTextCreate = "create window MyWindowOne#keepall as select * from RevType";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean_S1");
    
            epService.EPAdministrator.CreateEPL("create index MyWindowOneIndex1 on MyWindowOne(P10)");
            epService.EPAdministrator.CreateEPL("create index MyWindowOneIndex2 on MyWindowOne(P00)");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P00"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "P10"));
    
            epService.EPRuntime.ExecuteQuery("select * from MyWindowOne where P10='1'");
        }
    }
} // end of namespace
