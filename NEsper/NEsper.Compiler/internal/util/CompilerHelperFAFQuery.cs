///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperFAFQuery
    {
        public static string CompileQuery(
            FAFQueryMethodForge query,
            string classPostfix,
            ModuleCompileTimeServices compileTimeServices,
            out Assembly assembly)
        {
            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementFields),
                classPostfix);
            var packageScope = new CodegenNamespaceScope(
                compileTimeServices.Namespace,
                statementFieldsClassName,
                compileTimeServices.IsInstrumented());

            var queryMethodProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(FAFQueryMethodProvider),
                classPostfix);
            var forgeablesQueryMethod = query.MakeForgeables(queryMethodProviderClassName, classPostfix, packageScope);

            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>(forgeablesQueryMethod);
            forgeables.Add(new StmtClassForgeableStmtFields(statementFieldsClassName, packageScope, 0));

            // forge with statement-fields last
            var classes = new List<CodegenClass>(forgeables.Count);
            foreach (var forgeable in forgeables) {
                var clazz = forgeable.Forge(true, true);
                classes.Add(clazz);
            }

            // assign the assembly (required for completeness)
            assembly = null;

            // compile with statement-field first
            classes = classes
                .OrderBy(c => c.ClassType.GetSortCode())
                .ToList();

            var compiler = new RoslynCompiler()
                .WithCodeLogging(compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
                .WithCodeAuditDirectory(compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
                .WithCodegenClasses(classes);

            assembly = compiler.Compile();

            return queryMethodProviderClassName;
        }
    }
} // end of namespace