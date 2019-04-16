///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.compile
{
    public class IndexDetailForge
    {
        private readonly IndexMultiKey indexMultiKey;
        private readonly QueryPlanIndexItemForge queryPlanIndexItem;

        public IndexDetailForge(
            IndexMultiKey indexMultiKey,
            QueryPlanIndexItemForge queryPlanIndexItem)
        {
            this.indexMultiKey = indexMultiKey;
            this.queryPlanIndexItem = queryPlanIndexItem;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleIndexesInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(IndexDetail), indexMultiKey.Make(parent, classScope), queryPlanIndexItem.Make(parent, classScope));
        }
    }
} // end of namespace