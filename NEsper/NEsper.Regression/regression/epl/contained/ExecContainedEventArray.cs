///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.contained
{
    public class ExecContainedEventArray : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertion(epService);
            RunDocSample(epService);
        }
    
        private void RunDocSample(EPServiceProvider epService) {
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                    "create schema IdContainer(id int);" +
                            "create schema MyEvent(ids int[]);" +
                            "select * from MyEvent[ids@Type(IdContainer)];");
    
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                    "create window MyWindow#keepall (id int);" +
                            "on MyEvent[ids@Type(IdContainer)] as my_ids \n" +
                            "delete from MyWindow my_window \n" +
                            "where my_ids.id = my_window.id;");
        }
    
        private void RunAssertion(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanArrayCollMap));
    
            string epl = "create objectarray schema DeleteId(id int);" +
                    "create window MyWindow#keepall as SupportBean;" +
                    "insert into MyWindow select * from SupportBean;" +
                    "on SupportBeanArrayCollMap[intArr@Type(DeleteId)] delete from MyWindow where IntPrimitive = id";
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            AssertCount(epService, 2);
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[]{1, 2}));
            AssertCount(epService, 0);
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    
        private void AssertCount(EPServiceProvider epService, long i) {
            Assert.AreEqual(i, epService.EPRuntime.ExecuteQuery("select count(*) as c0 from MyWindow").Array[0].Get("c0"));
        }
    }
} // end of namespace
