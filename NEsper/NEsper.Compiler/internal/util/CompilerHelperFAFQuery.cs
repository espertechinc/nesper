///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
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
            string @namespace,
            ModuleCompileTimeServices compileTimeServices,
            out Assembly assembly)
        {
            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementFields),
                classPostfix);
            var packageScope = new CodegenNamespaceScope(
                @namespace,
                statementFieldsClassName,
                compileTimeServices.IsInstrumented());

            var queryMethodProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(FAFQueryMethodProvider),
                classPostfix);
            var forgablesQueryMethod = query.MakeForgables(queryMethodProviderClassName, classPostfix, packageScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>(forgablesQueryMethod);
            forgables.Add(new StmtClassForgableStmtFields(statementFieldsClassName, packageScope, 0));

            // forge with statement-fields last
            var classes = new List<CodegenClass>(forgables.Count);
            foreach (var forgable in forgables) {
                var clazz = forgable.Forge(true);
                classes.Add(clazz);
            }

            // assign the assembly (required for completeness)
            assembly = null;

            // compile with statement-field first
            classes.Sort(
                (
                        o1,
                        o2) => o1.InterfaceImplemented == typeof(StatementFields) ? -1 : 0);

            var compiler = new RoslynCompiler()
                .WithCodeLogging(compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
                .WithCodegenClasses(classes);
            assembly = compiler.Compile();

            return queryMethodProviderClassName;
        }
    }
} // end of namespace