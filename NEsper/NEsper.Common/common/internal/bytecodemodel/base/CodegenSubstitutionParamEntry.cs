///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenSubstitutionParamEntry
    {
        public CodegenSubstitutionParamEntry(
            CodegenField field,
            string name,
            Type type)
        {
            Field = field;
            Name = name;
            Type = type;
        }

        public CodegenField Field { get; }

        public string Name { get; }

        public Type Type { get; }

        public static void CodegenSetterMethod(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            var numbered = classScope.NamespaceScope.SubstitutionParamsByNumber;
            var named = classScope.NamespaceScope.SubstitutionParamsByName;
            if (numbered.IsEmpty() && named.IsEmpty()) {
                return;
            }

            if (!numbered.IsEmpty() && !named.IsEmpty()) {
                throw new IllegalStateException("Both named and numbered substitution parameters are non-empty");
            }

            IList<CodegenSubstitutionParamEntry> fields;
            if (!numbered.IsEmpty()) {
                fields = numbered;
            }
            else {
                fields = new List<CodegenSubstitutionParamEntry>(named.Values);
            }

            method.Block.DeclareVar<int>("zidx", Op(Ref("index"), "-", Constant(1)));
            var blocks = method.Block.SwitchBlockOfLength("zidx", fields.Count, false);
            for (var i = 0; i < blocks.Length; i++) {
                CodegenSubstitutionParamEntry param = fields[i];
                blocks[i].AssignRef(Field(param.Field), Cast(param.Type.GetBoxedType(), Ref("value")));
            }
        }
    }
} // end of namespace