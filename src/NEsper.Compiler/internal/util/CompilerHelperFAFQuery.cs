///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerHelperFAFQuery
	{
		public static string CompileQuery(
			FAFQueryMethodForge query,
			string classPostfix,
			CompilerAbstractionArtifactCollection compilerState,
			ModuleCompileTimeServices compileTimeServices,
			CompilerPath path)
		{
			var statementFieldsClassName =
				CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
			var namespaceScope = new CodegenNamespaceScope(
				compileTimeServices.Namespace,
				statementFieldsClassName,
				compileTimeServices.IsInstrumented,
				compileTimeServices.Configuration.Compiler.ByteCode);

			var queryMethodProviderClassName =
				CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(FAFQueryMethodProvider), classPostfix);
			var forgeablesQueryMethod = query.MakeForgeables(
				queryMethodProviderClassName,
				classPostfix,
				namespaceScope);

			IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>(forgeablesQueryMethod);
			forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, namespaceScope));

			// forge with statement-fields last
			var classes = new List<CodegenClass>(forgeables.Count);
			foreach (var forgeable in forgeables) {
				var clazz = forgeable.Forge(true, true);
				if (clazz == null) {
					continue;
				}

				classes.Add(clazz);
			}

			// compile with statement-field first
			classes = classes
				.OrderBy(c => c.ClassType.GetSortCode())
				.ToList();

			// remove statement field initialization when unused
			namespaceScope.RewriteStatementFieldUse(classes);

#if NOT_DOTNET
			// add class-provided create-class to classpath
			compileTimeServices.ClassProvidedCompileTimeResolver.AddTo(_ => compilerState.Add(_));
#endif

			var ctx = new CompilerAbstractionCompilationContext(compileTimeServices, path.Compileds);
			compileTimeServices.CompilerAbstraction.CompileClasses(classes, ctx, compilerState);

#if NOT_DOTNET
			// remove path create-class class-provided byte code
			compileTimeServices.ClassProvidedCompileTimeResolver.RemoveFrom(_ => compilerState.Remove(_));
#endif
			
			return queryMethodProviderClassName;
		}
	}
} // end of namespace
