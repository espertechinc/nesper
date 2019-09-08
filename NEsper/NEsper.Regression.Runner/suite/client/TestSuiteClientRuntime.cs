///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.client.runtime;
using com.espertech.esper.regressionlib.suite.@event.infra;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionrun.Runner;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientRuntime
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

        private void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanWithEnum),
                typeof(SupportMarketDataBean),
                typeof(SupportMarkerInterface),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_C),
                typeof(SupportBean_D),
                typeof(SupportBean_S0)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddEventType(
                ClientRuntimeListener.MAP_TYPENAME,
                Collections.SingletonDataMap("Ident", "string"));
            configuration.Common.AddEventType(
                ClientRuntimeListener.OA_TYPENAME,
                new[] { "Ident" },
                new[] {typeof(string)});
            configuration.Common.AddEventType(
                ClientRuntimeListener.BEAN_TYPENAME,
                typeof(ClientRuntimeListener.RoutedBeanEvent));

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"Ident\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(ClientRuntimeListener.XML_TYPENAME, eventTypeMeta);

            Schema avroSchema = SchemaBuilder.Record(
                EventInfraPropertyUnderlyingSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "Ident",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            configuration.Common.AddEventTypeAvro(
                ClientRuntimeListener.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));

            configuration.Common.AddImportType(typeof(MyAnnotationValueEnumAttribute));
            configuration.Common.AddImportNamespace(typeof(MyAnnotationNestableValuesAttribute));
            configuration.Common.AddAnnotationImportType(typeof(SupportEnum));

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "myinvalidagg",
                typeof(SupportInvalidAggregationFunctionForge));

            // add service (not serializable, transient configuration)
            var transients = new Dictionary<string, object>();
            transients.Put(
                ClientRuntimeItself.TEST_SERVICE_NAME,
                new ClientRuntimeItself.MyLocalService(ClientRuntimeItself.TEST_SECRET_VALUE));
            configuration.Common.TransientConfiguration = transients;

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Runtime.Execution.IsPrioritized = true;
        }

        [Test]
        public void TestClientRuntimeEPStatement()
        {
            RegressionRunner.Run(session, ClientRuntimeEPStatement.Executions());
        }

        [Test]
        public void TestClientRuntimeExceptionHandler()
        {
            RegressionRunner.Run(session, ClientRuntimeExceptionHandler.Executions());
        }

        [Test]
        public void TestClientRuntimeItself()
        {
            RegressionRunner.Run(session, ClientRuntimeItself.Executions());
        }

        [Test]
        public void TestClientRuntimeListener()
        {
            RegressionRunner.Run(session, ClientRuntimeListener.Executions());
        }

        [Test]
        public void TestClientRuntimePriorityAndDropInstructions()
        {
            RegressionRunner.Run(session, ClientRuntimePriorityAndDropInstructions.Executions());
        }

        [Test]
        public void TestClientRuntimeRuntimeProvider()
        {
            RegressionRunner.Run(session, ClientRuntimeRuntimeProvider.Executions());
        }

        [Test]
        public void TestClientRuntimeSolutionPatternPortScan()
        {
            RegressionRunner.Run(session, ClientRuntimeSolutionPatternPortScan.Executions());
        }

        [Test]
        public void TestClientRuntimeStatementAnnotation()
        {
            RegressionRunner.Run(session, ClientRuntimeStatementAnnotation.Executions());
        }

        [Test]
        public void TestClientRuntimeStatementName()
        {
            RegressionRunner.Run(session, ClientRuntimeStatementName.Executions());
        }

        [Test]
        public void TestClientRuntimeSubscriber()
        {
            RegressionRunner.Run(session, new ClientRuntimeSubscriber());
        }

        [Test]
        public void TestClientRuntimeTimeControl()
        {
            RegressionRunner.Run(session, ClientRuntimeTimeControl.Executions());
        }

        [Test]
        public void TestClientRuntimeUnmatchedListener()
        {
            RegressionRunner.Run(session, ClientRuntimeUnmatchedListener.Executions());
        }
    }
} // end of namespace