///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationTableAccessAggReaderCodegenField : CodegenFieldSharable
    {
        private readonly AggregationTableAccessAggReaderForge readerForge;
        private readonly CodegenClassScope classScope;
        private readonly Type generator;

        public AggregationTableAccessAggReaderCodegenField(
            AggregationTableAccessAggReaderForge readerForge,
            CodegenClassScope classScope,
            Type generator)
        {
            this.readerForge = readerForge;
            this.classScope = classScope;
            this.generator = generator;
        }

        public Type Type()
        {
            return typeof(AggregationMultiFunctionTableReader);
        }

        public CodegenExpression InitCtorScoped()
        {
            var symbols = new SAIFFInitializeSymbol();
            var init = classScope.NamespaceScope.InitMethod
                .MakeChildWithScope(typeof(AggregationMultiFunctionTableReader), generator, symbols, classScope).AddParam(
                    typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
            init.Block.MethodReturn(readerForge.CodegenCreateReader(init, symbols, classScope));
            return LocalMethod(init, EPStatementInitServicesConstants.REF);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (AggregationTableAccessAggReaderCodegenField) o;

            return readerForge.Equals(that.readerForge);
        }

        public override int GetHashCode()
        {
            return readerForge.GetHashCode();
        }
    }
} // end of namespace