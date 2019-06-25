///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
            string packageName,
            IDictionary<string, byte[]> moduleBytes,
            ModuleCompileTimeServices compileTimeServices)
        {
            string statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            CodegenNamespaceScope packageScope = new CodegenNamespaceScope(
                packageName, statementFieldsClassName, compileTimeServices.IsInstrumented());

            string queryMethodProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(FAFQueryMethodProvider), classPostfix);
            IList<StmtClassForgable> forgablesQueryMethod = query.MakeForgables(queryMethodProviderClassName, classPostfix, packageScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>(forgablesQueryMethod);
            forgables.Add(new StmtClassForgableStmtFields(statementFieldsClassName, packageScope, 0));

            // forge with statement-fields last
            List<CodegenClass> classes = new List<CodegenClass>(forgables.Count);
            foreach (StmtClassForgable forgable in forgables)
            {
                CodegenClass clazz = forgable.Forge(true);
                classes.Add(clazz);
            }

            // compile with statement-field first
            classes.Sort((o1, o2) => o1.InterfaceImplemented == typeof(StatementFields) ? -1 : 0);
            foreach (CodegenClass clazz in classes)
            {
                RoslynCompiler.Compile(clazz, moduleBytes, compileTimeServices.Configuration.Compiler.Logging.IsEnableCode);
            }

            return queryMethodProviderClassName;
        }
    }
} // end of namespace