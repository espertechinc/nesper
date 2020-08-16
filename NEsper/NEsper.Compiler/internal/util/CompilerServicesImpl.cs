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
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerServicesImpl : CompilerServices
    {
        public StatementSpecRaw ParseWalk(
            string epl,
            StatementSpecMapEnv mapEnv)
        {
            return CompilerHelperSingleEPL.ParseWalk(epl, mapEnv);
        }

        public string LexSampleSQL(string querySQL)
        {
            return SQLLexer.LexSampleSQL(querySQL);
        }

        public ExprNode CompileExpression(
            string expression,
            StatementCompileTimeServices services)
        {
            var toCompile = "select * from System.Object#time(" + expression + ")";

            StatementSpecRaw raw;
            try {
                raw = services.CompilerServices.ParseWalk(toCompile, services.StatementSpecMapEnv);
            }
            catch (StatementSpecCompileException e) {
                throw new ExprValidationException(
                    "Failed to compile expression '" + expression + "': " + e.Expression,
                    e);
            }

            return raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
        }

        public Type CompileStandInClass(
            CodegenClassType classType,
            string classNameSimple,
            ModuleCompileTimeServices services)
        {
            var namespaceScope = new CodegenNamespaceScope(services.Namespace, null, false);
            var classScope = new CodegenClassScope(true, namespaceScope, null);
            var clazz = new CodegenClass(
                classType,
                null,
                classNameSimple,
                classScope,
                EmptyList<CodegenTypedParam>.Instance,
                null,
                new CodegenClassMethods(),
                new CodegenClassProperties(),
                EmptyList<CodegenInnerClass>.Instance);
            
            // this mechanism is not fully baked, we need a better mechanism that allows us to output
            // the contents of the assembly that we are generating items into.

            throw new NotImplementedException();
        }

        public CompileResponse Compile(CompileRequest request)
        {
            var configuration = request.ModuleCompileTimeServices.Configuration;
            var compiler = new RoslynCompiler()
                .WithCodeLogging(configuration.Compiler.Logging.IsEnableCode)
                .WithCodeAuditDirectory(configuration.Compiler.Logging.AuditDirectory)
                .WithSources(request.Classes
                    .Select(_ => new RoslynCompiler.SourceBasic(_.ClassName, _.Code))
                    .ToList<RoslynCompiler.Source>());

            return new CompileResponse(
                request,
                compiler.Compile());

#if false
            try {
                JaninoCompiler.compile(code, filenameWithoutExtension, classpath, output, services);
            }
            catch (RuntimeException ex) {
                string message = ex.getMessage().replace(CompileException.class.getName() + ": ", "");
                throw new CompilerServicesCompileException(message, ex);
            }
#endif
        }
    }
} // end of namespace