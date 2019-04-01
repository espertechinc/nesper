///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableLifecycle : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionIntoTable(epService);
    
            RunAssertionDependent(epService, "create index IDX on mytable (p)");
            RunAssertionDependent(epService, "select * from SupportBean, mytable");
            RunAssertionDependent(epService, "select * from SupportBean where exists (select * from mytable)");
            RunAssertionDependent(epService, "insert into mytable select 'a' as id, 'a' as p from SupportBean");
        }
    
        private void RunAssertionIntoTable(EPServiceProvider epService) {
            string eplCreate = "create table abc (total count(*))";
            string eplUse = "select abc from SupportBean";
            string eplInto = "into table abc select count(*) as total from SupportBean";
    
            // typical select-use-destroy
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(eplCreate);
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(eplUse);
            EPStatement stmtInto = epService.EPAdministrator.CreateEPL(eplInto);
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__public"));
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__internal"));
    
            stmtCreate.Dispose();
            stmtSelect.Dispose();
            AssertFailCreate(epService, eplCreate);
            stmtInto.Dispose();
    
            // destroy-all
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL(eplInto);
            epService.EPAdministrator.CreateEPL(eplUse);
            epService.EPAdministrator.DestroyAllStatements();
    
            stmtCreate = epService.EPAdministrator.CreateEPL(eplCreate);
            stmtCreate.Dispose();
    
            // deploy and undeploy as module
            string module = eplCreate + ";\n" + eplUse + ";\n" + eplInto + ";\n";
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(module);
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__public"));
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__internal"));
    
            AssertFailCreate(epService, eplCreate);
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
            Assert.IsNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__public"));
            Assert.IsNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__internal"));
    
            // stop and start
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(eplCreate);
            stmtCreateTwo.Stop();
            AssertFailCreate(epService, eplCreate);
            stmtCreateTwo.Start();
            AssertFailCreate(epService, eplCreate);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDependent(EPServiceProvider epService, string eplDependent) {
            string eplCreate = "create table mytable (id string primary key, p string)";
    
            // typical select-use-destroy
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(eplCreate);
            EPStatement stmtDependent = epService.EPAdministrator.CreateEPL(eplDependent);
    
            stmtCreate.Dispose();
            AssertFailCreate(epService, eplCreate);
            stmtDependent.Dispose();
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertFailCreate(EPServiceProvider epService, string create) {
            try {
                epService.EPAdministrator.CreateEPL(create);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    }
} // end of namespace
