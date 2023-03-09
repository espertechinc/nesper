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

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile;
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
            var optionalPriorClasses = (optionalPrior?.Classes ?? EmptyList<Type>.Instance);
            var existingTypes = new Dictionary<String, Type>();
            existingTypes.PutAll(optionalPriorClasses.Select(_ => new KeyValuePair<string, Type>(_.FullName, _)));
            //var existingTypesSet = new HashSet<string>(existingTypes.Select(_ => _.FullName));

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

            ICompileArtifact artifact;
            try {
                artifact = compileTimeServices.CompilerServices.Compile(
                    new CompileRequest(compilables, compileTimeServices.Services));
            } 
            catch(CompilerServicesCompileException ex) {
                throw HandleException(ex, "Failed to compile class");
            }

            foreach (var exportedTypeName in artifact.TypeNames) {
                if (existingTypes.ContainsKey(exportedTypeName)) {
                    throw new ExprValidationException("Duplicate class by name '" + exportedTypeName + "'");
                }
            }
            
            // it's not entirely clear to me why we are loading the classes into the
            // current context as this is a compile time context.  These classes will
            // be materialized in the default load context

            IRuntimeArtifact runtimeArtifact = artifact.Runtime;
            foreach (var exportedType in runtimeArtifact.Assembly.ExportedTypes) {
                existingTypes.Add(exportedType.FullName, exportedType);
            }

            return new ClassProvidedPrecompileResult(runtimeArtifact, existingTypes.Values.ToList());
        }

        private static ExprValidationException HandleException(
            Exception ex,
            string action)
        {
            return new ExprValidationException(action + ": " + ex.Message, ex);
        }
    }
} // end of namespace