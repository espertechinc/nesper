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
        private int _column;
        private AggregationPortableValidation _aggregationPortableValidation;
        private string _aggregationExpression;
        private bool _methodAgg;
        private EPChainableType _optionalEnumerationType;

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
            this._column = column;
            this._aggregationPortableValidation = aggregationPortableValidation;
            this._aggregationExpression = aggregationExpression;
            this._methodAgg = methodAgg;
            this._optionalEnumerationType = optionalEnumerationType;
        }

        protected override CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetadataColumnAggregation), GetType(), classScope);
            method.Block.DeclareVarNewInstance(typeof(TableMetadataColumnAggregation), "col");
            MakeSettersInline(Ref("col"), method.Block);
            method.Block.SetProperty(Ref("col"), "Column", Constant(_column))
                .SetProperty(
                    Ref("col"),
                    "AggregationPortableValidation",
                    _aggregationPortableValidation.Make(method, symbols, classScope))
                .SetProperty(Ref("col"), "AggregationExpression", Constant(_aggregationExpression))
                .SetProperty(Ref("col"), "MethodAgg", Constant(_methodAgg))
                .SetProperty(
                    Ref("col"),
                    "OptionalEnumerationType",
                    _optionalEnumerationType == null
                        ? ConstantNull()
                        : _optionalEnumerationType.Codegen(method, classScope, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("col"));
            return LocalMethod(method);
        }

        public bool IsMethodAgg => _methodAgg;

        public int Column {
            get => _column;

            set => _column = value;
        }

        public AggregationPortableValidation AggregationPortableValidation {
            get => _aggregationPortableValidation;

            set => _aggregationPortableValidation = value;
        }

        public string AggregationExpression {
            get => _aggregationExpression;

            set => _aggregationExpression = value;
        }

        public bool MethodAgg {
            set => _methodAgg = value;
        }

        public EPChainableType OptionalEnumerationType {
            get => _optionalEnumerationType;

            set => _optionalEnumerationType = value;
        }
    }
} // end of namespace