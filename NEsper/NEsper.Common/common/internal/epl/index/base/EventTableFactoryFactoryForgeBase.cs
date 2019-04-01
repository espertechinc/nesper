///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public abstract class EventTableFactoryFactoryForgeBase : EventTableFactoryFactoryForge
    {
        internal readonly int indexedStreamNum;
        internal readonly int? subqueryNum;
        internal readonly bool isFireAndForget;

        public abstract Type EventTableClass { get; }
        public abstract string ToQueryPlan();

        protected abstract Type TypeOf();
        protected abstract IList<CodegenExpression> AdditionalParams(
            CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope);

        public EventTableFactoryFactoryForgeBase(int indexedStreamNum, int? subqueryNum, bool isFireAndForget)
        {
            this.indexedStreamNum = indexedStreamNum;
            this.subqueryNum = subqueryNum;
            this.isFireAndForget = isFireAndForget;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(EventTableFactoryFactory), this.GetType(), classScope);
            IList<CodegenExpression> @params = new List<CodegenExpression>();
            @params.Add(Constant(indexedStreamNum));
            @params.Add(Constant(subqueryNum));
            @params.Add(ConstantNull());
            @params.Add(Constant(isFireAndForget));
            @params.AddAll(AdditionalParams(method, symbols, classScope));
            method.Block.MethodReturn(NewInstance(TypeOf(), @params.ToArray()));
            return LocalMethod(method);
        }
    }
} // end of namespace