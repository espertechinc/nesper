///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryInformationals
    {
        private readonly Type[] _substitutionParamsTypes;
        private readonly IDictionary<string, int> _substitutionParamsNames;

        public FAFQueryInformationals(
            Type[] substitutionParamsTypes,
            IDictionary<string, int> substitutionParamsNames)
        {
            _substitutionParamsTypes = substitutionParamsTypes;
            _substitutionParamsNames = substitutionParamsNames;
        }

        public static FAFQueryInformationals From(
            IList<CodegenSubstitutionParamEntry> paramsByNumber,
            IDictionary<string, CodegenSubstitutionParamEntry> paramsByName)
        {
            Type[] types;
            IDictionary<string, int> names;
            if (!paramsByNumber.IsEmpty()) {
                types = new Type[paramsByNumber.Count];
                for (var i = 0; i < paramsByNumber.Count; i++) {
                    types[i] = paramsByNumber[i].EntryType;
                }

                names = null;
            }
            else if (!paramsByName.IsEmpty()) {
                types = new Type[paramsByName.Count];
                names = new Dictionary<string, int>();
                var index = 0;
                foreach (var entry in paramsByName) {
                    types[index] = entry.Value.EntryType;
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

        public Type[] SubstitutionParamsTypes => _substitutionParamsTypes;

        public IDictionary<string, int> SubstitutionParamsNames => _substitutionParamsNames;

        public void Make(
            CodegenBlock block,
            CodegenClassScope classScope)
        {
            if (SubstitutionParamsNames == null) {
                block.DeclareVar<IDictionary<string, int>>("names", ConstantNull());
            }
            else {
                block.DeclareVar<IDictionary<string, int>>("names", NewInstance<Dictionary<string, int>>());
                foreach (var entry in SubstitutionParamsNames) {
                    block.AssignArrayElement("names", Constant(entry.Key), Constant(entry.Value));
                }
            }

            block.BlockReturn(
                NewInstance<FAFQueryInformationals>(
                    Constant(SubstitutionParamsTypes),
                    Ref("names")));
        }
        
        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return NewInstance<FAFQueryInformationals>(
                Constant(_substitutionParamsTypes),
                MakeNames(parent, classScope));
        }

        private CodegenExpression MakeNames(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            if (_substitutionParamsNames == null) {
                return ConstantNull();
            }

            var method = parent.MakeChild(typeof(IDictionary<string, int>), GetType(), classScope);
            method.Block.DeclareVar(
                typeof(IDictionary<string, int>),
                "names",
                NewInstance(
                    typeof(Dictionary<string, int>)));

            new CodegenRepetitiveValueBuilder<KeyValuePair<string, int>>(_substitutionParamsNames, method, classScope, GetType())
                .AddParam(typeof(IDictionary<string, int>), "names")
                .SetConsumer(
                    (
                        entry,
                        index,
                        leaf) => {
                        leaf.Block.ExprDotMethod(Ref("names"), "Put", Constant(entry.Key), Constant(entry.Value));
                    })
                .Build();
            method.Block.MethodReturn(Ref("names"));
            return LocalMethod(method);
        }
    }
} // end of namespace