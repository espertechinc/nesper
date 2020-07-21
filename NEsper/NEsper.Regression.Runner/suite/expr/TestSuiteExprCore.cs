///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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

using SupportBeanComplexProps = com.espertech.esper.common.@internal.support.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.common.@internal.support.SupportMarkerInterface;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprCore
    {
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

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBeanArrayCollMap), typeof(SupportBeanComplexProps),
                typeof(SupportBean_StringAlphabetic), typeof(SupportMarkerInterface),
                typeof(SupportBeanDynRoot), typeof(SupportMarketDataBean), typeof(SupportBeanWithEnum), typeof(SupportEnumTwo),
                typeof(SupportEventTypeErasure), typeof(SupportChainTop), typeof(SupportLevelZero), typeof(SupportEventNode),
                typeof(SupportEventNodeData), typeof(SupportBeanCombinedProps), typeof(SupportBeanNumeric),
                typeof(ISupportA), typeof(ISupportABCImpl), typeof(ISupportAImpl), typeof(SupportBean_ST0), typeof(SupportBeanObject),
                typeof(SupportEventWithManyArray), typeof(SupportBeanWithArray), typeof(SupportBean_S0)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddEventType(
                "MyDateType",
                CollectionUtil.PopulateNameValueMap(
                    "yyyymmdd",
                    typeof(string),
                    "yyyymmddhhmmss",
                    typeof(string),
                    "hhmmss",
                    typeof(string),
                    "yyyymmddhhmmsszz",
                    typeof(string)));

            configuration.Common.AddImportType(typeof(SupportBean));
            configuration.Common.AddImportType(typeof(SupportEnum));
            configuration.Common.AddImportType(typeof(SupportPrivateCtor));
            configuration.Common.AddImportType(typeof(SupportObjectCtor));
            configuration.Common.AddImportType(typeof(SupportEnumTwo));
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            var map = new Dictionary<string, object>();
            map.Put("anInt", typeof(string));
            map.Put("anDouble", typeof(string));
            map.Put("anLong", typeof(string));
            map.Put("anFloat", typeof(string));
            map.Put("anByte", typeof(string));
            map.Put("anShort", typeof(string));
            map.Put("intPrimitive", typeof(int));
            map.Put("intBoxed", typeof(int?));
            configuration.Common.AddEventType("StaticTypeMapEvent", map);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }

        [Test]
        public void TestExprCoreAndOrNot()
        {
            RegressionRunner.Run(session, ExprCoreAndOrNot.Executions());
        }

        [Test]
        public void TestExprCoreAnyAllSome()
        {
            RegressionRunner.Run(session, ExprCoreAnyAllSome.Executions());
        }

        [Test]
        public void TestExprCoreArray()
        {
            RegressionRunner.Run(session, ExprCoreArray.Executions());
        }

        [Test]
        public void TestExprCoreArrayAtElement()
        {
            RegressionRunner.Run(session, ExprCoreArrayAtElement.Executions());
        }

        [Test]
        public void TestExprCoreBigNumberSupport()
        {
            RegressionRunner.Run(session, ExprCoreBigNumberSupport.Executions());
        }

        [Test]
        public void TestExprCoreBitWiseOperators()
        {
            RegressionRunner.Run(session, ExprCoreBitWiseOperators.Executions());
        }

        [Test]
        public void TestExprCoreCase()
        {
            RegressionRunner.Run(session, ExprCoreCase.Executions());
        }

        [Test]
        public void TestExprCoreCast()
        {
            RegressionRunner.Run(session, ExprCoreCast.Executions());
        }

        [Test]
        public void TestExprCoreCoalesce()
        {
            RegressionRunner.Run(session, ExprCoreCoalesce.Executions());
        }

        [Test]
        public void TestExprCoreConcat()
        {
            RegressionRunner.Run(session, new ExprCoreConcat());
        }

        [Test]
        public void TestExprCoreCurrentEvaluationContext()
        {
            RegressionRunner.Run(session, ExprCoreCurrentEvaluationContext.Executions());
        }

        [Test]
        public void TestExprCoreCurrentTimestamp()
        {
            RegressionRunner.Run(session, ExprCoreCurrentTimestamp.Executions());
        }

        [Test]
        public void TestExprCoreDotExpression()
        {
            RegressionRunner.Run(session, ExprCoreDotExpression.Executions());
        }

        [Test]
        public void TestExprCoreEqualsIs()
        {
            RegressionRunner.Run(session, ExprCoreEqualsIs.Executions());
        }

        [Test]
        public void TestExprCoreExists()
        {
            RegressionRunner.Run(session, ExprCoreExists.Executions());
        }

        [Test]
        public void TestExprCoreInBetween()
        {
            RegressionRunner.Run(session, ExprCoreInBetween.Executions());
        }

        [Test]
        public void TestExprCoreInstanceOf()
        {
            RegressionRunner.Run(session, ExprCoreInstanceOf.Executions());
        }

        [Test]
        public void TestExprCoreLikeRegexp()
        {
            RegressionRunner.Run(session, ExprCoreLikeRegexp.Executions());
        }

        [Test]
        public void TestExprCoreMath()
        {
            RegressionRunner.Run(session, ExprCoreMath.Executions());
        }

        [Test]
        public void TestExprCoreMinMaxNonAgg()
        {
            RegressionRunner.Run(session, ExprCoreMinMaxNonAgg.Executions());
        }

        [Test]
        public void TestExprCoreNewInstance()
        {
            RegressionRunner.Run(session, ExprCoreNewInstance.Executions());
        }

        [Test]
        public void TestExprCoreNewStruct()
        {
            RegressionRunner.Run(session, ExprCoreNewStruct.Executions());
        }

        [Test]
        public void TestExprCorePrevious()
        {
            RegressionRunner.Run(session, ExprCorePrevious.Executions());
        }

        [Test]
        public void TestExprCorePrior()
        {
            RegressionRunner.Run(session, ExprCorePrior.Executions());
        }

        [Test]
        public void TestExprCoreRelOp()
        {
            RegressionRunner.Run(session, new ExprCoreRelOp());
        }

        [Test]
        public void TestExprCoreTypeOf()
        {
            RegressionRunner.Run(session, ExprCoreTypeOf.Executions());
        }

        [Test]
        public void TestExprEventIdentityEquals()
        {
            RegressionRunner.Run(session, ExprCoreEventIdentityEquals.Executions());
        }
    }
} // end of namespace