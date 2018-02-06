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

namespace com.espertech.esper.regression.context
{
    public class ExecContextPartitionedPrioritized : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.EngineDefaults.Execution.IsPrioritized = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(
                    "create context SegmentedByMessage partition by TheString from SupportBean");
    
            EPStatement statementWithDropAnnotation = epService.EPAdministrator.CreateEPL(
                    "@Drop @Priority(1) context SegmentedByMessage select 'test1' from SupportBean");
            var statementWithDropAnnotationListener = new SupportUpdateListener();
            statementWithDropAnnotation.Events += statementWithDropAnnotationListener.Update;
    
            EPStatement lowPriorityStatement = epService.EPAdministrator.CreateEPL(
                    "@Priority(0) context SegmentedByMessage select 'test2' from SupportBean");
            var lowPriorityStatementListener = new SupportUpdateListener();
            lowPriorityStatement.Events += lowPriorityStatementListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("test msg", 1));
    
            Assert.IsTrue(statementWithDropAnnotationListener.IsInvoked);
            Assert.IsFalse(lowPriorityStatementListener.IsInvoked);
        }
    
    }
} // end of namespace
