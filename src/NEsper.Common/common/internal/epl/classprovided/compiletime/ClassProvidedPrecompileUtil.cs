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

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public class ClassProvidedPrecompileUtil
    {
        public static ClassProvidedPrecompileResult CompileClassProvided(
            IList<string> classTexts,
            StatementCompileTimeServices compileTimeServices,
            ClassProvidedPrecompileResult optionalPrior)
        {
            if (classTexts == null || classTexts.IsEmpty()) {
                return ClassProvidedPrecompileResult.EMPTY;
            }

            if (!compileTimeServices.Configuration.Compiler.ByteCode.IsAllowInlinedClass) {
                throw new ExprValidationException("Inlined-class compilation has been disabled by configuration");
            }

            var index = -1;
            var existingTypes = new List<Type>(optionalPrior?.Classes ?? EmptyList<Type>.Instance);
            var existingTypesSet = new HashSet<string>(existingTypes.Select(_ => _.FullName));

            // In .NET our classes must be part of an assembly.  This is different from Java, where each class 
            // can be compiled into its own .class file.  Technically, we can create netmodules, but even then
            // its a container for classes.

            var compilables = new List<CompilableClass>();

            foreach (var classText in classTexts.Where(_ => !string.IsNullOrWhiteSpace(_))) {
                index++;

                var classNameId = CodeGenerationIDGenerator.GenerateClassNameUUID();
                var className = $"provided_{index}_{classNameId}";
                compilables.Add(new CompilableClass(classText, className));
            }

            CompileResponse response;
            try {
                response = compileTimeServices.CompilerServices.Compile(
                    new CompileRequest(compilables, compileTimeServices.Services));
            } 
            catch(CompilerServicesCompileException ex) {
                throw HandleException(ex, "Failed to compile class");
            }

            foreach (var exportedType in response.Assembly.ExportedTypes) {
                if (existingTypesSet.Contains(exportedType.FullName)) {
                    throw new ExprValidationException("Duplicate class by name '" + exportedType.FullName + "'");
                }

                existingTypes.Add(exportedType);
            }

            return new ClassProvidedPrecompileResult(response.Assembly, existingTypes);
        }

        private static ExprValidationException HandleException(
            Exception ex,
            string action)
        {
            return new ExprValidationException(action + ": " + ex.Message, ex);
        }
    }
} // end of namespace