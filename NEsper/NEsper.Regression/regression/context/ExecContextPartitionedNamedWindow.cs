///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextPartitionedNamedWindow : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean");
    
            epService.EPAdministrator.CreateEPL("context SegmentedByString create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("context SegmentedByString insert into MyWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
    
            string expected = "Error executing statement: Named window 'MyWindow' is associated to context 'SegmentedByString' that is not available for querying without context partition selector, use the ExecuteQuery(epl, selector) method instead [select * from MyWindow]";
            try {
                epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            } catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            EPOnDemandPreparedQueryParameterized prepared = epService.EPRuntime.PrepareQueryWithParameters("select * from MyWindow");
            try {
                epService.EPRuntime.ExecuteQuery(prepared);
            } catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
        }
    }
} // end of namespace
