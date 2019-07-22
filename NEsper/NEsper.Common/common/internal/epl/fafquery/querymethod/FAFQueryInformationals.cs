///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryInformationals
    {
        public FAFQueryInformationals(
            Type[] substitutionParamsTypes,
            IDictionary<string, int> substitutionParamsNames)
        {
            SubstitutionParamsTypes = substitutionParamsTypes;
            SubstitutionParamsNames = substitutionParamsNames;
        }

        public Type[] SubstitutionParamsTypes { get; }

        public IDictionary<string, int> SubstitutionParamsNames { get; }

        public static FAFQueryInformationals From(
            IList<CodegenSubstitutionParamEntry> paramsByNumber,
            IDictionary<string, CodegenSubstitutionParamEntry> paramsByName)
        {
            Type[] types;
            IDictionary<string, int> names;
            if (!paramsByNumber.IsEmpty()) {
                types = new Type[paramsByNumber.Count];
                for (var i = 0; i < paramsByNumber.Count; i++) {
                    types[i] = paramsByNumber[i].Type;
                }

                names = null;
            }
            else if (!paramsByName.IsEmpty()) {
                types = new Type[paramsByName.Count];
                names = new Dictionary<string, int>();
                var index = 0;
                foreach (var entry in paramsByName) {
                    types[index] = entry.Value.Type;
                    names.Put(entry.Key, index + 1);
                    index++;
                }
            }
            else {
                types = null;
                names = null;
            }

            return new FAFQueryInformationals(types, names);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return NewInstance<FAFQueryInformationals>(
                Constant(SubstitutionParamsTypes),
                MakeNames(parent, classScope));
        }

        private CodegenExpression MakeNames(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            if (SubstitutionParamsNames == null) {
                return ConstantNull();
            }

            var method = parent.MakeChild(typeof(IDictionary<object, object>), GetType(), classScope);
            method.Block.DeclareVar<IDictionary<object, object>>(
                "names",
                NewInstance(
                    typeof(Dictionary<object, object>),
                    Constant(CollectionUtil.CapacityHashMap(SubstitutionParamsNames.Count))));
            foreach (var entry in SubstitutionParamsNames) {
                method.Block.ExprDotMethod(Ref("names"), "put", Constant(entry.Key), Constant(entry.Value));
            }

            method.Block.MethodReturn(Ref("names"));
            return LocalMethod(method);
        }
    }
} // end of namespace