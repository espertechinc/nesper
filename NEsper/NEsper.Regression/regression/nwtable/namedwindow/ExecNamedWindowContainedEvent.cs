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
using com.espertech.esper.supportregression.bean.bookexample;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowContainedEvent : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(OrderBean));
    
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            epService.EPAdministrator.CreateEPL("create window OrderWindow#time(30) as OrderBean");
    
            try {
                string epl = "select * from SupportBean unidirectional, OrderWindow[books]";
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate named window use in join, contained-event is only allowed for named windows when marked as unidirectional [select * from SupportBean unidirectional, OrderWindow[books]]", ex.Message);
            }
    
            try {
                string epl = "select *, (select bookId from OrderWindow[books] where sb.TheString = bookId) " +
                        "from SupportBean sb";
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to plan subquery number 1 querying OrderWindow: Failed to validate named window use in subquery, contained-event is only allowed for named windows when not correlated [select *, (select bookId from OrderWindow[books] where sb.TheString = bookId) from SupportBean sb]", ex.Message);
            }
        }
    }
} // end of namespace
