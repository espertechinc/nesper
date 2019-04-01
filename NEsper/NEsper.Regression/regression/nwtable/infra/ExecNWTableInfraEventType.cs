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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraEventType : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionType(epService, true);
            RunAssertionType(epService, false);
    
            // name cannot be the same as an existing event type
            epService.EPAdministrator.CreateEPL("create schema SchemaOne as (p0 string)");
            SupportMessageAssertUtil.TryInvalid(epService, "create window SchemaOne.win:keepall as SchemaOne",
                    "Error starting statement: An event type or schema by name 'SchemaOne' already exists"
            );
    
            epService.EPAdministrator.CreateEPL("create schema SchemaTwo as (p0 string)");
            SupportMessageAssertUtil.TryInvalid(epService, "create table SchemaTwo(c0 int)",
                    "Error starting statement: An event type or schema by name 'SchemaTwo' already exists"
            );
        }
    
        private void RunAssertionType(EPServiceProvider epService, bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as (c0 int[], c1 int[primitive])" :
                    "create table MyInfra (c0 int[], c1 int[primitive])";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, false, eplCreate);
    
            var expectedType = new object[][]{new object[] {"c0", typeof(int[])}, new object[] {"c1", typeof(int[])}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmt.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
} // end of namespace
