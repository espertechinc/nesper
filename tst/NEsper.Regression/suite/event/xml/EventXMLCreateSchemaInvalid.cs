///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
	public class EventXMLCreateSchemaInvalid : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        env.TryInvalidCompile("create xml schema ABC()",
	            "Required annotation @XMLSchema could not be found");

	        env.TryInvalidCompile("@XMLSchema(rootElementName='a') create xml schema ABC(prop string)",
	            "Create-XML-Schema does not allow specifying columns, use @XMLSchemaField instead");

	        env.TryInvalidCompile("@XMLSchema(rootElementName='') create xml schema ABC()",
	            "Required annotation field 'rootElementName' for annotation @XMLSchema could not be found");

	        env.TryInvalidCompile("@XMLSchema(rootElementName='abc') create xml schema Base();\n" +
	                "@XMLSchema(rootElementName='abc') create xml schema ABC() copyfrom Base;\n",
	                "Create-XML-Schema does not allow copy-from");

	        env.TryInvalidCompile("@XMLSchema(rootElementName='abc') create xml schema Base();\n" +
	                "@XMLSchema(rootElementName='abc') create xml schema ABC() inherits Base;\n",
	                "Create-XML-Schema does not allow inherits");

	        env.TryInvalidCompile("@XMLSchema(rootElementName='abc') @XMLSchema(rootElementName='def') create xml schema Base()",
	                "Found multiple @XMLSchema annotations but expected a single annotation");
	    }
	}
} // end of namespace
