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

namespace com.espertech.esper.regression.client
{
    public class ExecClientIsolationUnitConfig : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
        }
    
        public override void Run(EPServiceProvider epService) {
            try {
                epService.GetEPServiceIsolated("i1");
                Assert.Fail();
            } catch (EPServiceNotAllowedException ex) {
                Assert.AreEqual("Isolated runtime requires execution setting to allow isolated services, please change execution settings under engine defaults", ex.Message);
            }
        }
    }
} // end of namespace
