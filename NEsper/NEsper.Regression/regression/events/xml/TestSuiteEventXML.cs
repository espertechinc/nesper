///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.xml
{
    [TestFixture]
    public class TestSuiteEventXML
    {
        [Test]
        public void TestExecEventXMLNoSchemaEventTransposeXPathConfigured() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaEventTransposeXPathConfigured());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaEventTransposeXPathGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaEventTransposeXPathGetter());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaEventTransposeDOM() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaEventTransposeDOM());
        }
    
        [Test]
        public void TestExecEventXMLSchemaPropertyDynamicXPathGetter() {
            RegressionRunner.Run(new ExecEventXMLSchemaPropertyDynamicXPathGetter());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventObservationDOM() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventObservationDOM());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventObservationXPath() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventObservationXPath());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventReplace() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventReplace());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventSender() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventSender());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventTransposeDOMGetter() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventTransposeDOMGetter());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventTransposeXPathConfigured() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventTransposeXPathConfigured());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventTransposeXPathGetter() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventTransposeXPathGetter());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventTransposePrimitiveArray() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventTransposePrimitiveArray());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventTransposeNodeArray() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventTransposeNodeArray());
        }
    
        [Test]
        public void TestExecEventXMLSchemaEventTypes() {
            RegressionRunner.Run(new ExecEventXMLSchemaEventTypes());
        }
    
        [Test]
        public void TestExecEventXMLSchemaWithRestriction() {
            RegressionRunner.Run(new ExecEventXMLSchemaWithRestriction());
        }
    
        [Test]
        public void TestExecEventXMLSchemaWithAll() {
            RegressionRunner.Run(new ExecEventXMLSchemaWithAll());
        }
    
        [Test]
        public void TestExecEventXMLSchemaDOMGetterBacked() {
            RegressionRunner.Run(new ExecEventXMLSchemaDOMGetterBacked());
        }
    
        [Test]
        public void TestExecEventXMLSchemaXPathBacked() {
            RegressionRunner.Run(new ExecEventXMLSchemaXPathBacked());
        }
    
        [Test]
        public void TestExecEventXMLSchemaAddRemoveType() {
            RegressionRunner.Run(new ExecEventXMLSchemaAddRemoveType());
        }
    
        [Test]
        public void TestExecEventXMLSchemaInvalid() {
            RegressionRunner.Run(new ExecEventXMLSchemaInvalid());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaVariableAndDotMethodResolution() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaVariableAndDotMethodResolution());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaSimpleXMLXPathProperties() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaSimpleXMLXPathProperties());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaSimpleXMLDOMGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaSimpleXMLDOMGetter());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaSimpleXMLXPathGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaSimpleXMLXPathGetter());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaNestedXMLDOMGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaNestedXMLDOMGetter());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaNestedXMLXPathGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaNestedXMLXPathGetter());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaDotEscape() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaDotEscape());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaEventXML() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaEventXML());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaElementNode() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaElementNode());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaNamespaceXPathRelative() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaNamespaceXPathRelative());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaNamespaceXPathAbsolute() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaNamespaceXPathAbsolute());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaXPathArray() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaXPathArray());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaPropertyDynamicDOMGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaPropertyDynamicDOMGetter());
        }
    
        [Test]
        public void TestExecEventXMLNoSchemaPropertyDynamicXPathGetter() {
            RegressionRunner.Run(new ExecEventXMLNoSchemaPropertyDynamicXPathGetter());
        }
    
        [Test]
        public void TestExecEventXMLSchemaPropertyDynamicDOMGetter() {
            RegressionRunner.Run(new ExecEventXMLSchemaPropertyDynamicDOMGetter());
        }
    }
} // end of namespace
