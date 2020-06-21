///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
    public class ClassProvided
    {
        public ClassProvided()
        {
        }

        public ClassProvided(
            Assembly assembly,
            string className)
        {
            Assembly = assembly;
            ClassName = className;
        }
        
        public Assembly Assembly { get; set; }

        public IEnumerable<Type> Types => Assembly.GetExportedTypes();
        
        public string ModuleName { get; set; }

        public NameAccessModifier Visibility { get; set; } = NameAccessModifier.TRANSIENT;

        public IList<Type> ClassesMayNull { get; private set; }

        public string ClassName { get; set; }

        public void LoadClasses(ClassLoader parentClassLoader)
        {
            ClassesMayNull = new List<Type>();
            
            // The assembly is the container for all the types, there is no need to explicitly
            // load them as is done in Java.  In Esper, the class is loaded as a byte array which
            // the ByteArrayProvidingClassProvider must initialize.

            foreach (var clazz in Assembly.GetExportedTypes()) {
                ClassesMayNull.Add(clazz);
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ClassProvided), GetType(), classScope);
            if (Assembly == null) {
                method.Block.DeclareVar<Assembly>(
                    "assembly",
                    ConstantNull());
            }
            else {
                method.Block.DeclareVar<Assembly>(
                    "assembly",
                    ExprDotMethod(
                        EnumValue(typeof(AppDomain), "CurrentDomain"),
                        "Load",
                        Constant(Assembly.FullName)));
            }

            method.Block
                .DeclareVar<ClassProvided>("cp", NewInstance(typeof(ClassProvided)))
                .SetProperty(Ref("cp"), "Assembly", Ref("assembly"))
                .SetProperty(Ref("cp"), "ClassName", Constant(ClassName))
                .SetProperty(Ref("cp"), "ModuleName", Constant(ModuleName))
                .SetProperty(Ref("cp"), "Visibility", Constant(Visibility))
                .MethodReturn(Ref("cp"));
            return LocalMethod(method);
        }
    }
} // end of namespace