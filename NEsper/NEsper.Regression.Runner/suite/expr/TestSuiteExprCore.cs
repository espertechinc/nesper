///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.expr.exprcore;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprCore
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreRelOp()
        {
            RegressionRunner.Run(session, new ExprCoreRelOp());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreAndOrNot()
        {
            RegressionRunner.Run(session, new ExprCoreAndOrNot());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreConcat()
        {
            RegressionRunner.Run(session, new ExprCoreConcat());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreLikeRegexp()
        {
            RegressionRunner.Run(session, ExprCoreLikeRegexp.Executions());
        }

        [Test]
        public void TestExprCoreMath()
        {
            RegressionRunner.Run(session, ExprCoreMath.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreInBetweenLike()
        {
            RegressionRunner.Run(session, ExprCoreInBetweenLike.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreArray()
        {
            RegressionRunner.Run(session, ExprCoreArray.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreAnyAllSome()
        {
            RegressionRunner.Run(session, ExprCoreAnyAllSome.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreBitWiseOperators()
        {
            RegressionRunner.Run(session, ExprCoreBitWiseOperators.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreCoalesce()
        {
            RegressionRunner.Run(session, ExprCoreCoalesce.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreNewInstance()
        {
            RegressionRunner.Run(session, ExprCoreNewInstance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreCast()
        {
            RegressionRunner.Run(session, ExprCoreCast.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreCase()
        {
            RegressionRunner.Run(session, ExprCoreCase.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreCurrentTimestamp()
        {
            RegressionRunner.Run(session, ExprCoreCurrentTimestamp.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreEqualsIs()
        {
            RegressionRunner.Run(session, ExprCoreEqualsIs.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreInstanceOf()
        {
            RegressionRunner.Run(session, ExprCoreInstanceOf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreExists()
        {
            RegressionRunner.Run(session, ExprCoreExists.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreNewStruct()
        {
            RegressionRunner.Run(session, ExprCoreNewStruct.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreDotExpression()
        {
            RegressionRunner.Run(session, ExprCoreDotExpression.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreMinMaxNonAgg()
        {
            RegressionRunner.Run(session, ExprCoreMinMaxNonAgg.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreBigNumberSupport()
        {
            RegressionRunner.Run(session, ExprCoreBigNumberSupport.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreCurrentEvaluationContext()
        {
            RegressionRunner.Run(session, ExprCoreCurrentEvaluationContext.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreTypeOf()
        {
            RegressionRunner.Run(session, ExprCoreTypeOf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCorePrevious()
        {
            RegressionRunner.Run(session, ExprCorePrevious.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprCorePrior()
        {
            RegressionRunner.Run(session, ExprCorePrior.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBeanArrayCollMap),
                typeof(SupportBeanComplexProps),
                typeof(SupportBean_StringAlphabetic),
                typeof(SupportMarkerInterface),
                typeof(SupportBeanDynRoot),
                typeof(SupportMarketDataBean),
                typeof(SupportBeanWithEnum),
                typeof(SupportEnumTwo),
                typeof(SupportEventTypeErasure),
                typeof(SupportChainTop),
                typeof(SupportLevelZero),
                typeof(SupportEventNode),
                typeof(SupportEventNodeData),
                typeof(SupportBeanCombinedProps),
                typeof(SupportBeanNumeric),
                typeof(ISupportA),
                typeof(ISupportABCImpl),
                typeof(ISupportAImpl),
                typeof(SupportBean_ST0),
                typeof(SupportBeanObject)})
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddEventType("MyDateType", CollectionUtil.PopulateNameValueMap("yyyymmdd", typeof(string), "yyyymmddhhmmss", typeof(string), "hhmmss", typeof(string), "yyyymmddhhmmssvv", typeof(string)));

            configuration.Common.AddImportType(typeof(SupportBean));
            configuration.Common.AddImportType(typeof(SupportEnum));
            configuration.Common.AddImportType(typeof(SupportPrivateCtor));
            configuration.Common.AddImportType(typeof(SupportObjectCtor));
            configuration.Common.AddImportType(typeof(SupportEnumTwo));
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Put("anInt", typeof(string));
            map.Put("anDouble", typeof(string));
            map.Put("anLong", typeof(string));
            map.Put("anFloat", typeof(string));
            map.Put("anByte", typeof(string));
            map.Put("anShort", typeof(string));
            map.Put("IntPrimitive", typeof(int));
            map.Put("IntBoxed", typeof(int?));
            configuration.Common.AddEventType("StaticTypeMapEvent", map);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }
    }
} // end of namespace