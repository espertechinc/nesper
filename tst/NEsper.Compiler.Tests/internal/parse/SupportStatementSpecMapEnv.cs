///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.script.compiletime;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class SupportStatementSpecMapEnv
	{
		public static StatementSpecMapEnv Make(ImportServiceCompileTime engineImportService)
		{
			return new StatementSpecMapEnv(
				engineImportService,
				VariableCompileTimeResolverEmpty.INSTANCE,
				new Configuration(),
				ExprDeclaredCompileTimeResolverEmpty.INSTANCE,
				ContextCompileTimeResolverEmpty.INSTANCE,
				TableCompileTimeResolverEmpty.INSTANCE,
				ScriptCompileTimeResolverEmpty.INSTANCE,
				null,
				new ClassProvidedExtensionImpl(ClassProvidedCompileTimeResolverEmpty.INSTANCE));
		}

		public static StatementSpecMapEnv Make(IContainer container)
		{
			return Make(SupportClasspathImport.GetInstance(container));
		}
	}
} // end of namespace
