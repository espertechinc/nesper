///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLCreateSchemaInvalid : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TryInvalidCompile(
                env,
                "create xml schema ABC()",
                "Required annotation @XMLSchemaAttribute could not be found");

            TryInvalidCompile(
                env,
                "@XMLSchema(RootElementName='a') create xml schema ABC(prop string)",
                "Create-XML-Schema does not allow specifying columns, use @XMLSchemaFieldAttribute instead");

            TryInvalidCompile(
                env,
                "@XMLSchema(RootElementName='') create xml schema ABC()",
                "Required annotation field 'RootElementName' for annotation @XMLSchemaAttribute could not be found");

            TryInvalidCompile(
                env,
                "@XMLSchema(RootElementName='abc') create xml schema Base();\n" +
                "@XMLSchema(RootElementName='abc') create xml schema ABC() copyfrom Base;\n",
                "Create-XML-Schema does not allow copy-from");

            TryInvalidCompile(
                env,
                "@XMLSchema(RootElementName='abc') create xml schema Base();\n" +
                "@XMLSchema(RootElementName='abc') create xml schema ABC() inherits Base;\n",
                "Create-XML-Schema does not allow inherits");

            TryInvalidCompile(
                env,
                "@XMLSchema(RootElementName='abc') @XMLSchema(RootElementName='def') create xml schema Base()",
                "Found multiple @XMLSchemaAttribute annotations but expected a single annotation");
        }
    }
} // end of namespace