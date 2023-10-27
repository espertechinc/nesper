///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
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
            EntryType = type;
        }

        public CodegenField Field { get; }

        public string Name { get; }

        public Type EntryType { get; }

        public static void CodegenSetterBody(
            CodegenClassScope classScope,
            CodegenMethodScope methodScope,
            CodegenBlock enclosingBlock,
            CodegenExpression stmtFieldsInstance)
        {
            var targetMethodComplexity = Math.Max(
                64,
                classScope.NamespaceScope.Config.InternalUseOnlyMaxMethodComplexity);
            var numbered = classScope.NamespaceScope.SubstitutionParamsByNumber;
            var named = classScope.NamespaceScope.SubstitutionParamsByName;
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

            if (fields.Count <= targetMethodComplexity) {
                PopulateSet(enclosingBlock, fields, 0);
                return;
            }

            var assignments = CollectionUtil.Subdivide(
                fields,
                targetMethodComplexity);
            var leafs = new List<CodegenMethod>(assignments.Count);
            for (var i = 0; i < assignments.Count; i++) {
                var assignment = assignments[i];
                var leaf = methodScope
                    .MakeChild(typeof(void), typeof(CodegenSubstitutionParamEntry), classScope)
                    .AddParam<int>("index")
                    .AddParam<object>("value");
                PopulateSet(leaf.Block, assignment, i * targetMethodComplexity);
                leafs.Add(leaf);
            }

            enclosingBlock.DeclareVar(
                typeof(int),
                "lidx",
                Op(Op(Ref("index"), "-", Constant(1)), "/", Constant(targetMethodComplexity)));
            var blocks = enclosingBlock.SwitchBlockOfLength(Ref("lidx"), assignments.Count, false);
            for (var i = 0; i < blocks.Length; i++) {
                blocks[i].LocalMethod(leafs[i], Ref("index"), Ref("value"));
            }
        }

        private static void PopulateSet(
            CodegenBlock block,
            IList<CodegenSubstitutionParamEntry> fields,
            int offset)
        {
            block.DeclareVar(typeof(int), "zidx", Op(Ref("index"), "-", Constant(1)));
            var blocks = block.SwitchBlockOfLength(Ref("zidx"), fields.Count, false, offset);
            for (var i = 0; i < blocks.Length; i++) {
                var param = fields[i];
                blocks[i].AssignRef(Field(param.Field), Cast(param.EntryType.GetBoxedType(), Ref("value")));
            }
        }
    }
} // end of namespace