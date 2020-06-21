///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
    public class ClassProvided
    {
        public ClassProvided()
        {
        }

        public ClassProvided(
            IDictionary<string, byte[]> bytes,
            string className)
        {
            Bytes = bytes;
            ClassName = className;
        }

        public IDictionary<string, byte[]> Bytes { get; set; }

        public string ModuleName { get; set; }

        public NameAccessModifier Visibility { get; set; } = NameAccessModifier.TRANSIENT;

        public IList<Type> ClassesMayNull { get; private set; }

        public string ClassName { get; set; }

        public void LoadClasses(ClassLoader parentClassLoader)
        {
            ClassesMayNull = new List<Type>(2);
            ByteArrayProvidingClassLoader cl = new ByteArrayProvidingClassLoader(Bytes, parentClassLoader);
            foreach (KeyValuePair<string, byte[]> entry in Bytes.EntrySet()) {
                try {
                    Type clazz = Type.ForName(entry.Key, false, cl);
                    ClassesMayNull.Add(clazz);
                }
                catch (TypeLoadException e) {
                    throw new EPException("Unexpected exception loading class " + entry.Key + ": " + e.Message, e);
                }
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ClassProvided), GetType(), classScope);
            if (Bytes.IsEmpty()) {
                method.Block.DeclareVar(typeof(IDictionary<string, object>), "bytes", StaticMethod(typeof(Collections), "emptyMap"));
            }
            else {
                method.Block.DeclareVar(
                    typeof(IDictionary<string, object>),
                    "bytes",
                    NewInstance(typeof(Dictionary<string, object>), Constant(CollectionUtil.CapacityHashMap(Bytes.Count))));
                foreach (var entry in Bytes) {
                    method.Block.ExprDotMethod(
                        Ref("bytes"),
                        "put",
                        Constant(entry.Key),
                        Constant(entry.Value));
                }
            }

            method.Block
                .DeclareVar(typeof(ClassProvided), "cp", NewInstance(typeof(ClassProvided)))
                .ExprDotMethod(Ref("cp"), "setBytes", Ref("bytes"))
                .ExprDotMethod(Ref("cp"), "setClassName", Constant(ClassName))
                .ExprDotMethod(Ref("cp"), "setModuleName", Constant(ModuleName))
                .ExprDotMethod(Ref("cp"), "setVisibility", Constant(Visibility))
                .MethodReturn(Ref("cp"));
            return LocalMethod(method);
        }
    }
} // end of namespace