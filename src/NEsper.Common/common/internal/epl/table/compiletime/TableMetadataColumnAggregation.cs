///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableMetadataColumnAggregation : TableMetadataColumn
    {
        private int column;
        private AggregationPortableValidation aggregationPortableValidation;
        private string aggregationExpression;
        private bool methodAgg;
        private EPChainableType optionalEnumerationType;

        public TableMetadataColumnAggregation()
        {
        }

        public TableMetadataColumnAggregation(
            string columnName,
            bool key,
            int column,
            AggregationPortableValidation aggregationPortableValidation,
            string aggregationExpression,
            bool methodAgg,
            EPChainableType optionalEnumerationType) : base(columnName, key)
        {
            this.column = column;
            this.aggregationPortableValidation = aggregationPortableValidation;
            this.aggregationExpression = aggregationExpression;
            this.methodAgg = methodAgg;
            this.optionalEnumerationType = optionalEnumerationType;
        }

        protected override CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetadataColumnAggregation), GetType(), classScope);
            method.Block.DeclareVarNewInstance(typeof(TableMetadataColumnAggregation), "col");
            MakeSettersInline(Ref("col"), method.Block);
            method.Block.SetProperty(Ref("col"), "Column", Constant(column))
                .SetProperty(
                    Ref("col"),
                    "AggregationPortableValidation",
                    aggregationPortableValidation.Make(method, symbols, classScope))
                .SetProperty(Ref("col"), "AggregationExpression", Constant(aggregationExpression))
                .SetProperty(Ref("col"), "MethodAgg", Constant(methodAgg))
                .SetProperty(
                    Ref("col"),
                    "OptionalEnumerationType",
                    optionalEnumerationType == null
                        ? ConstantNull()
                        : optionalEnumerationType.Codegen(method, classScope, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("col"));
            return LocalMethod(method);
        }

        public bool IsMethodAgg => methodAgg;

        public int Column {
            get => column;

            set => column = value;
        }

        public AggregationPortableValidation AggregationPortableValidation {
            get => aggregationPortableValidation;

            set => aggregationPortableValidation = value;
        }

        public string AggregationExpression {
            get => aggregationExpression;

            set => aggregationExpression = value;
        }

        public bool MethodAgg {
            set => methodAgg = value;
        }

        public EPChainableType OptionalEnumerationType {
            get => optionalEnumerationType;

            set => optionalEnumerationType = value;
        }
    }
} // end of namespace