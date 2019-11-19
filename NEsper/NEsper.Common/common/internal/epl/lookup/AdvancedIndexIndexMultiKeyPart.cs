///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public class AdvancedIndexIndexMultiKeyPart
    {
        public AdvancedIndexIndexMultiKeyPart(
            string indexTypeName,
            string[] indexExpressions,
            string[] indexedProperties)
        {
            IndexTypeName = indexTypeName;
            IndexExpressions = indexExpressions;
            IndexedProperties = indexedProperties;
        }

        public string IndexTypeName { get; }

        public string[] IndexExpressions { get; }

        public string[] IndexedProperties { get; }

        public bool EqualsAdvancedIndex(AdvancedIndexIndexMultiKeyPart that)
        {
            return IndexTypeName.Equals(that.IndexTypeName) && IndexExpressions.AreEqual(that.IndexExpressions);
        }

        public string ToQueryPlan()
        {
            if (IndexExpressions.Length == 0) {
                return IndexTypeName;
            }

            var writer = new StringWriter();
            writer.Write(IndexTypeName);
            writer.Write("(");
            writer.Write(string.Join(",", IndexExpressions));
            writer.Write(")");
            return writer.ToString();
        }

        public CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return NewInstance<AdvancedIndexIndexMultiKeyPart>(
                Constant(IndexTypeName),
                Constant(IndexExpressions),
                Constant(IndexedProperties));
        }
    }
} // end of namespace