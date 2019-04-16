///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryIndexDescForge
    {
        public SubordinateQueryIndexDescForge(
            IndexKeyInfo optionalIndexKeyInfo,
            string indexName,
            string indexModuleName,
            IndexMultiKey indexMultiKey,
            QueryPlanIndexItemForge optionalQueryPlanIndexItem)
        {
            OptionalIndexKeyInfo = optionalIndexKeyInfo;
            IndexName = indexName;
            IndexModuleName = indexModuleName;
            IndexMultiKey = indexMultiKey;
            OptionalQueryPlanIndexItem = optionalQueryPlanIndexItem;
        }

        public IndexKeyInfo OptionalIndexKeyInfo { get; }

        public string IndexName { get; }

        public string IndexModuleName { get; }

        public IndexMultiKey IndexMultiKey { get; }

        public QueryPlanIndexItemForge OptionalQueryPlanIndexItem { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubordinateQueryIndexDesc), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(IndexMultiKey), "indexMultiKey", IndexMultiKey.Make(method, classScope))
                .DeclareVar(
                    typeof(QueryPlanIndexItem), "queryPlanIndexItem",
                    OptionalQueryPlanIndexItem == null
                        ? ConstantNull()
                        : OptionalQueryPlanIndexItem.Make(method, classScope));
            method.Block.MethodReturn(
                NewInstance(
                    typeof(SubordinateQueryIndexDesc),
                    ConstantNull(), Constant(IndexName), Ref("indexMultiKey"), Ref("queryPlanIndexItem")));
            return LocalMethod(method);
        }
    }
} // end of namespace