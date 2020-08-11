///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
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
            var allBytes = new Dictionary<string, byte[]>();
            var allClasses = new List<Type>();
            if (optionalPrior != null) {
                allBytes.PutAll(optionalPrior.Bytes);
            }

            ByteArrayProvidingClassLoader cl = new ByteArrayProvidingClassLoader(allBytes, compileTimeServices.Services.ParentClassLoader);

            foreach (var classText in classTexts) {
                if (string.IsNullOrEmpty(classText)) {
                    continue;
                }

                index++;
                var className = "provided_" + index + "_" + CodeGenerationIDGenerator.GenerateClassNameUUID();
                IDictionary<string, byte[]> output = new Dictionary<string, byte[]>();

                try {
                    compileTimeServices.CompilerServices.CompileClass(classText, className, allBytes, output, compileTimeServices.Services);
                }
                catch (CompilerServicesCompileException ex) {
                    throw HandleException(ex, "Failed to compile class", classText);
                }

                foreach (var entry in output) {
                    if (allBytes.ContainsKey(entry.Key)) {
                        throw new ExprValidationException("Duplicate class by name '" + entry.Key + "'");
                    }
                }

                allBytes.PutAll(output);
                IList<Type> classes = new List<Type>(2);
                foreach (var entry in output) {
                    try {
                        Type clazz = TypeHelper.GetClassForName(entry.Key, cl);
                        classes.Add(clazz);
                    }
                    catch (TypeLoadException e) {
                        throw HandleException(e, "Failed to load class '" + entry.Key + "'", classText);
                    }
                }

                allClasses.AddAll(classes);
            }

            return new ClassProvidedPrecompileResult(allBytes, allClasses);
        }

        private static ExprValidationException HandleException(
            Exception ex,
            string action,
            string classText)
        {
            return new ExprValidationException(action + ": " + ex.Message + " for class [\"\"\"" + classText + "\"\"\"]", ex);
        }
    }
} // end of namespace