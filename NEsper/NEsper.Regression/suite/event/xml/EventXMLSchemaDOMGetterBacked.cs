///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
	public class EventXMLSchemaDOMGetterBacked
	{
		public static IList<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			execs.Add(new EventXMLSchemaDOMGetterBackedPreconfig());
			execs.Add(new EventXMLSchemaDOMGetterBackedCreateSchema());
			return execs;
		}

		public class EventXMLSchemaDOMGetterBackedPreconfig : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				EventXMLSchemaXPathBacked.RunAssertion(env, false, "XMLSchemaConfigTwo", new RegressionPath());
			}
		}

		public class EventXMLSchemaDOMGetterBackedCreateSchema : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var resourceManager = env.Container.ResourceManager();
				var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
				var epl = "@public @buseventtype " +
				          "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" + schemaUriSimpleSchema + "', XPathPropertyExpr=false)" +
				          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='samples:schemas:simpleSchema')" +
				          "@XMLSchemaField(Name='customProp', XPath='count(/ss:simpleEvent/ss:nested3/ss:nested4)', Type='number')" +
				          "create xml schema MyEventCreateSchema()";
				var path = new RegressionPath();
				env.CompileDeploy(epl, path);
				EventXMLSchemaXPathBacked.RunAssertion(env, false, "MyEventCreateSchema", path);
			}
		}
	}
} // end of namespace