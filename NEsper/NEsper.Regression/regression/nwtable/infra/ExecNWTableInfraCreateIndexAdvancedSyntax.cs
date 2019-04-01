///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraCreateIndexAdvancedSyntax : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (Type clazz in Collections.List(typeof(SupportSpatialPoint))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            AssertCompileSODA(epService, "create index MyIndex on MyWindow((x,y) dummy_name(\"a\",10101))");
            AssertCompileSODA(epService, "create index MyIndex on MyWindow(x dummy_name)");
            AssertCompileSODA(epService, "create index MyIndex on MyWindow((x,y,z) dummy_name)");
            AssertCompileSODA(epService, "create index MyIndex on MyWindow(x dummy_name, (y,z) dummy_name_2(\"a\"), p dummyname3)");
    
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportSpatialPoint");
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow(())",
                    "Error starting statement: Invalid empty list of index expressions");
    
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow(IntPrimitive+1)",
                    "Error starting statement: Invalid index expression 'IntPrimitive+1'");
    
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow((x, y))",
                    "Error starting statement: Invalid multiple index expressions");
    
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow(x.y)",
                    "Error starting statement: Invalid index expression 'x.y'");
    
            SupportMessageAssertUtil.TryInvalid(epService, "create index MyIndex on MyWindow(id xxxx)",
                    "Error starting statement: Unrecognized advanced-type index 'xxxx'");
        }
    
        private void AssertCompileSODA(EPServiceProvider epService, string epl) {
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
        }
    }
} // end of namespace
