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
using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    public class OnSplitItemForge
    {
        private string resultSetProcessorClassName;

        public OnSplitItemForge(
            ExprNode whereClause,
            bool isNamedWindowInsert,
            TableMetaData insertIntoTable,
            ResultSetProcessorDesc resultSetProcessorDesc,
            PropertyEvaluatorForge propertyEvaluator)
        {
            WhereClause = whereClause;
            IsNamedWindowInsert = isNamedWindowInsert;
            InsertIntoTable = insertIntoTable;
            ResultSetProcessorDesc = resultSetProcessorDesc;
            PropertyEvaluator = propertyEvaluator;
        }

        public ExprNode WhereClause { get; }

        public bool IsNamedWindowInsert { get; }

        public TableMetaData InsertIntoTable { get; }

        public ResultSetProcessorDesc ResultSetProcessorDesc { get; }

        public PropertyEvaluatorForge PropertyEvaluator { get; }

        public string ResultSetProcessorClassName {
            set => resultSetProcessorClassName = value;
        }

        public static CodegenExpression Make(
            OnSplitItemForge[] items,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var expressions = new CodegenExpression[items.Length];
            for (var i = 0; i < items.Length; i++) {
                expressions[i] = items[i].Make(parent, symbols, classScope);
            }

            return NewArrayWithInit(typeof(OnSplitItemEval), expressions);
        }

        private CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(OnSplitItemEval), GetType(), classScope);
            method.Block
                .DeclareVar<OnSplitItemEval>("eval", NewInstance(typeof(OnSplitItemEval)))
                .SetProperty(
                    Ref("eval"),
                    "WhereClause",
                    WhereClause == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(WhereClause.Forge, method, GetType(), classScope))
                .SetProperty(Ref("eval"), "IsNamedWindowInsert", Constant(IsNamedWindowInsert))
                .SetProperty(
                    Ref("eval"),
                    "InsertIntoTable",
                    InsertIntoTable == null
                        ? ConstantNull()
                        : TableDeployTimeResolver.MakeResolveTable(InsertIntoTable, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("eval"),
                    "RspFactoryProvider",
                    NewInstance(resultSetProcessorClassName, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("eval"),
                    "PropertyEvaluator",
                    PropertyEvaluator == null ? ConstantNull() : PropertyEvaluator.Make(method, symbols, classScope))
                .MethodReturn(Ref("eval"));
            return LocalMethod(method);
        }
    }
} // end of namespace