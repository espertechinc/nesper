///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.fromclausemethod;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLFromClauseMethodWConfig
    {
        [Test]
        public void TestEPLFromClauseMethodCacheExpiry()
        {
            RegressionSession session = RegressionRunner.Session();

            ConfigurationCommonMethodRef methodConfig = new ConfigurationCommonMethodRef();
            methodConfig.SetExpiryTimeCache(1, 10);
            session.Configuration.Common.AddMethodRef(typeof(SupportStaticMethodInvocations), methodConfig);
            session.Configuration.Common.AddImportNamespace(typeof(SupportStaticMethodInvocations));
            session.Configuration.Common.AddEventType(typeof(SupportBean));

            RegressionRunner.Run(session, new EPLFromClauseMethodCacheExpiry());

            session.Destroy();
        }

        [Test]
        public void TestEPLFromClauseMethodCacheLRU()
        {
            RegressionSession session = RegressionRunner.Session();

            ConfigurationCommonMethodRef methodConfig = new ConfigurationCommonMethodRef();
            methodConfig.SetLRUCache(3);
            session.Configuration.Common.AddMethodRef(typeof(SupportStaticMethodInvocations), methodConfig);
            session.Configuration.Common.AddImportNamespace(typeof(SupportStaticMethodInvocations));
            session.Configuration.Common.AddEventType(typeof(SupportBean));

            RegressionRunner.Run(session, new EPLFromClauseMethodCacheLRU());

            session.Destroy();
        }

        [Test]
        public void TestEPLFromClauseMethodJoinPerformance()
        {
            RegressionSession session = RegressionRunner.Session();

            ConfigurationCommonMethodRef configMethod = new ConfigurationCommonMethodRef();
            configMethod.SetLRUCache(10);
            session.Configuration.Common.AddMethodRef(typeof(SupportJoinMethods), configMethod);
            session.Configuration.Common.AddEventType(typeof(SupportBeanInt));
            session.Configuration.Common.AddImportType(typeof(SupportJoinMethods));

            RegressionRunner.Run(session, EPLFromClauseMethodJoinPerformance.Executions());
            session.Destroy();
        }

        [Test]
        public void TestEPLFromClauseMethodVariable()
        {
            RegressionSession session = RegressionRunner.Session();

            Configuration configuration = session.Configuration;
            configuration.Common.AddMethodRef(typeof(EPLFromClauseMethodVariable.MyStaticService), new ConfigurationCommonMethodRef());

            configuration.Common.AddImportType(typeof(EPLFromClauseMethodVariable.MyStaticService));
            configuration.Common.AddImportType(typeof(EPLFromClauseMethodVariable.MyNonConstantServiceVariableFactory));
            configuration.Common.AddImportType(typeof(EPLFromClauseMethodVariable.MyNonConstantServiceVariable));

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("MyConstantServiceVariable", typeof(EPLFromClauseMethodVariable.MyConstantServiceVariable), new EPLFromClauseMethodVariable.MyConstantServiceVariable());
            common.AddVariable("MyNonConstantServiceVariable", typeof(EPLFromClauseMethodVariable.MyNonConstantServiceVariable), new EPLFromClauseMethodVariable.MyNonConstantServiceVariable("postfix"));
            common.AddVariable("MyNullMap", typeof(EPLFromClauseMethodVariable.MyMethodHandlerMap), null);
            common.AddVariable("MyMethodHandlerMap", typeof(EPLFromClauseMethodVariable.MyMethodHandlerMap), new EPLFromClauseMethodVariable.MyMethodHandlerMap("a", "b"));
            common.AddVariable("MyMethodHandlerOA", typeof(EPLFromClauseMethodVariable.MyMethodHandlerOA), new EPLFromClauseMethodVariable.MyMethodHandlerOA("a", "b"));

            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Common.AddEventType(typeof(SupportBean_S0));
            configuration.Common.AddEventType(typeof(SupportBean_S1));
            configuration.Common.AddEventType(typeof(SupportBean_S2));

            RegressionRunner.Run(session, EPLFromClauseMethodVariable.Executions());

            session.Destroy();
        }
    }
} // end of namespace