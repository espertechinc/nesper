///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestSuiteClient
    {
        [Test]
        public void TestExecClientAdapterLoader() {
            RegressionRunner.Run(new ExecClientAdapterLoader());
        }
    
        [Test]
        public void TestExecClientAggregationFunctionPlugIn() {
            RegressionRunner.Run(new ExecClientAggregationFunctionPlugIn());
        }
    
        [Test]
        public void TestExecClientAggregationMultiFunctionPlugIn() {
            RegressionRunner.Run(new ExecClientAggregationMultiFunctionPlugIn());
        }
    
        [Test]
        public void TestExecClientAudit() {
            RegressionRunner.Run(new ExecClientAudit());
        }
    
        [Test]
        public void TestExecClientMicrosecondResolution() {
            RegressionRunner.Run(new ExecClientMicrosecondResolution());
        }
    
        [Test]
        public void TestExecClientConfigurationOperations() {
            RegressionRunner.Run(new ExecClientConfigurationOperations());
        }
    
        [Test]
        public void TestExecClientDeployAdmin() {
            RegressionRunner.Run(new ExecClientDeployAdmin());
        }
    
        [Test]
        public void TestExecClientDeployOrder() {
            RegressionRunner.Run(new ExecClientDeployOrder());
        }
    
        [Test]
        public void TestExecClientDeployParse() {
            RegressionRunner.Run(new ExecClientDeployParse());
        }
    
        [Test]
        public void TestExecClientDeployRedefinition() {
            RegressionRunner.Run(new ExecClientDeployRedefinition());
        }
    
        [Test]
        public void TestExecClientEPAdministrator() {
            RegressionRunner.Run(new ExecClientEPAdministrator());
        }
    
        [Test]
        public void TestExecClientEPAdministratorPerformance() {
            RegressionRunner.Run(new ExecClientEPAdministratorPerformance());
        }
    
        [Test]
        public void TestExecClientEPStatement() {
            RegressionRunner.Run(new ExecClientEPStatement());
        }
    
        [Test]
        public void TestExecClientEPStatementObjectModel() {
            RegressionRunner.Run(new ExecClientEPStatementObjectModel());
        }
    
        [Test]
        public void TestExecClientEPStatementSubstitutionParams() {
            RegressionRunner.Run(new ExecClientEPStatementSubstitutionParams());
        }
    
        [Test]
        public void TestExecClientEPServiceProvider() {
            RegressionRunner.Run(new ExecClientEPServiceProvider());
        }
    
        [Test]
        public void TestExecClientEPServiceProviderMetricsJMX() {
            RegressionRunner.Run(new ExecClientEPServiceProviderMetricsJMX());
        }
    
        [Test]
        public void TestExecClientExceptionHandler() {
            RegressionRunner.Run(new ExecClientExceptionHandler());
        }
    
        [Test]
        public void TestExecClientExceptionHandlerNoHandler() {
            RegressionRunner.Run(new ExecClientExceptionHandlerNoHandler());
        }
    
        [Test]
        public void TestExecClientExceptionHandlerGetCtx() {
            RegressionRunner.Run(new ExecClientExceptionHandlerGetCtx());
        }
    
        [Test]
        public void TestExecClientInvalidSyntaxMsg() {
            RegressionRunner.Run(new ExecClientInvalidSyntaxMsg());
        }
    
        [Test]
        public void TestExecClientIsolationUnit() {
            RegressionRunner.Run(new ExecClientIsolationUnit());
        }
    
        [Test]
        public void TestExecClientIsolationUnitConfig() {
            RegressionRunner.Run(new ExecClientIsolationUnitConfig());
        }
    
        [Test]
        public void TestExecClientMetricsReportingNW() {
            RegressionRunner.Run(new ExecClientMetricsReportingNW());
        }
    
        [Test]
        public void TestExecClientMetricsReportingEngineMetrics() {
            RegressionRunner.Run(new ExecClientMetricsReportingEngineMetrics());
        }
    
        [Test]
        public void TestExecClientMetricsReportingStmtGroups() {
            RegressionRunner.Run(new ExecClientMetricsReportingStmtGroups());
        }
    
        [Test]
        public void TestExecClientMetricsReportingStmtMetrics() {
            RegressionRunner.Run(new ExecClientMetricsReportingStmtMetrics());
        }
    
        [Test]
        public void TestExecClientMetricsReportingDisableRuntime() {
            RegressionRunner.Run(new ExecClientMetricsReportingDisableRuntime());
        }
    
        [Test]
        public void TestExecClientMetricsReportingDisableStatement() {
            RegressionRunner.Run(new ExecClientMetricsReportingDisableStatement());
        }
    
        [Test]
        public void TestExecClientPatternGuardPlugIn() {
            RegressionRunner.Run(new ExecClientPatternGuardPlugIn());
        }
    
        [Test]
        public void TestExecClientPriorityAndDropInstructions() {
            RegressionRunner.Run(new ExecClientPriorityAndDropInstructions());
        }
    
        [Test]
        public void TestExecClientSingleRowFunctionPlugIn() {
            RegressionRunner.Run(new ExecClientSingleRowFunctionPlugIn());
        }
    
        [Test]
        public void TestExecClientSolutionPatternPortScan() {
            RegressionRunner.Run(new ExecClientSolutionPatternPortScan());
        }
    
        [Test]
        public void TestExecClientStatementAnnotation() {
            RegressionRunner.Run(new ExecClientStatementAnnotation());
        }
    
        [Test]
        public void TestExecClientStatementAnnotationImport() {
            RegressionRunner.Run(new ExecClientStatementAnnotationImport());
        }
    
        [Test]
        public void TestExecClientStatementAwareEvents() {
            RegressionRunner.Run(new ExecClientStatementAwareEvents());
        }
    
        [Test]
        public void TestExecClientSubscriberBind() {
            RegressionRunner.Run(new ExecClientSubscriberBind());
        }
    
        [Test]
        public void TestExecClientSubscriberInvalid() {
            RegressionRunner.Run(new ExecClientSubscriberInvalid());
        }
    
        [Test]
        public void TestExecClientSubscriberMgmt() {
            RegressionRunner.Run(new ExecClientSubscriberMgmt());
        }
    
        [Test]
        public void TestExecClientSubscriberPerf() {
            RegressionRunner.Run(new ExecClientSubscriberPerf());
        }
    
        [Test]
        public void TestExecClientThreadedConfigInbound() {
            RegressionRunner.Run(new ExecClientThreadedConfigInbound());
        }
    
        [Test]
        public void TestExecClientThreadedConfigInboundFastShutdown() {
            RegressionRunner.Run(new ExecClientThreadedConfigInboundFastShutdown());
        }
    
        [Test]
        public void TestExecClientThreadedConfigOutbound() {
            RegressionRunner.Run(new ExecClientThreadedConfigOutbound());
        }
    
        [Test]
        public void TestExecClientThreadedConfigRoute() {
            RegressionRunner.Run(new ExecClientThreadedConfigRoute());
        }
    
        [Test]
        public void TestExecClientThreadedConfigTimer() {
            RegressionRunner.Run(new ExecClientThreadedConfigTimer());
        }
    
        [Test]
        public void TestExecClientTimeControlEvent() {
            RegressionRunner.Run(new ExecClientTimeControlEvent());
        }
    
        [Test]
        public void TestExecClientConfigurationTransients() {
            RegressionRunner.Run(new ExecClientConfigurationTransients());
        }
    
        [Test]
        public void TestExecClientUnmatchedListener() {
            RegressionRunner.Run(new ExecClientUnmatchedListener());
        }
    
        [Test]
        public void TestExecClientViewPlugin() {
            RegressionRunner.Run(new ExecClientViewPlugin());
        }
    
        [Test]
        public void TestExecClientVirtualDataWindow() {
            RegressionRunner.Run(new ExecClientVirtualDataWindow());
        }
    
        [Test]
        public void TestExecClientVirtualDataWindowLateConsume() {
            RegressionRunner.Run(new ExecClientVirtualDataWindowLateConsume());
        }
    
        [Test]
        public void TestExecClientVirtualDataWindowToLookup() {
            RegressionRunner.Run(new ExecClientVirtualDataWindowToLookup());
        }
    }
} // end of namespace
