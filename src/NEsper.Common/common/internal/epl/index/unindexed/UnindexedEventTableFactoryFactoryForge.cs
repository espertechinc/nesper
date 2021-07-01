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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.unindexed
{
    public class UnindexedEventTableFactoryFactoryForge : EventTableFactoryFactoryForgeBase
    {
        public UnindexedEventTableFactoryFactoryForge(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget)
            : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
        }

        public override Type EventTableClass => typeof(UnindexedEventTable);

        public override string ToQueryPlan()
        {
            return GetType().Name + " streamNum=" + indexedStreamNum;
        }

        protected override Type TypeOf()
        {
            return typeof(UnindexedEventTableFactoryFactory);
        }

        protected override IList<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return Collections.GetEmptyList<CodegenExpression>();
        }
    }
} // end of namespace