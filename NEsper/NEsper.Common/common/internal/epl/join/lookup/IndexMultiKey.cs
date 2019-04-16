///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.lookup
{
    public class IndexMultiKey
    {
        public IndexMultiKey(
            bool unique,
            IList<IndexedPropDesc> hashIndexedProps,
            IList<IndexedPropDesc> rangeIndexedProps,
            AdvancedIndexIndexMultiKeyPart advancedIndexDesc)
        {
            IsUnique = unique;
            HashIndexedProps = hashIndexedProps.ToArray();
            RangeIndexedProps = rangeIndexedProps.ToArray();
            AdvancedIndexDesc = advancedIndexDesc;
        }

        public IndexMultiKey(
            bool unique,
            IndexedPropDesc[] hashIndexedProps,
            IndexedPropDesc[] rangeIndexedProps,
            AdvancedIndexIndexMultiKeyPart advancedIndexDesc)
        {
            IsUnique = unique;
            HashIndexedProps = hashIndexedProps;
            RangeIndexedProps = rangeIndexedProps;
            AdvancedIndexDesc = advancedIndexDesc;
        }

        public bool IsUnique { get; }

        public IndexedPropDesc[] HashIndexedProps { get; }

        public IndexedPropDesc[] RangeIndexedProps { get; }

        public AdvancedIndexIndexMultiKeyPart AdvancedIndexDesc { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(IndexMultiKey), GetType(), classScope);
            var hashes = IndexedPropDesc.MakeArray(HashIndexedProps);
            var ranges = IndexedPropDesc.MakeArray(RangeIndexedProps);
            var advanced = AdvancedIndexDesc == null
                ? ConstantNull()
                : AdvancedIndexDesc.CodegenMake(parent, classScope);
            method.Block.MethodReturn(NewInstance(typeof(IndexMultiKey), Constant(IsUnique), hashes, ranges, advanced));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            var writer = new StringWriter();
            writer.Write(IsUnique ? "unique " : "non-unique ");
            writer.Write("hash={");
            IndexedPropDesc.ToQueryPlan(writer, HashIndexedProps);
            writer.Write("} btree={");
            IndexedPropDesc.ToQueryPlan(writer, RangeIndexedProps);
            writer.Write("} advanced={");
            writer.Write(AdvancedIndexDesc == null ? "" : AdvancedIndexDesc.ToQueryPlan());
            writer.Write("}");
            return writer.ToString();
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (IndexMultiKey) o;

            if (IsUnique != that.IsUnique) {
                return false;
            }

            if (!CompatExtensions.AreEqual(HashIndexedProps, that.HashIndexedProps)) {
                return false;
            }

            if (!CompatExtensions.AreEqual(RangeIndexedProps, that.RangeIndexedProps)) {
                return false;
            }

            if (AdvancedIndexDesc == null) {
                return that.AdvancedIndexDesc == null;
            }

            return that.AdvancedIndexDesc != null && AdvancedIndexDesc.EqualsAdvancedIndex(that.AdvancedIndexDesc);
        }

        public override int GetHashCode()
        {
            int result = CompatExtensions.Hash(HashIndexedProps);
            result = 31 * result + CompatExtensions.Hash(RangeIndexedProps);
            return result;
        }
    }
} // end of namespace