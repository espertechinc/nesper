///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        private readonly ExprNode whereClause;
        private readonly bool isNamedWindowInsert;
        private readonly TableMetaData insertIntoTable;
        private readonly ResultSetProcessorDesc resultSetProcessorDesc;
        private readonly PropertyEvaluatorForge propertyEvaluator;
        private string resultSetProcessorClassName;
        private readonly ExprNode eventPrecedence;

        public OnSplitItemForge(
            ExprNode whereClause,
            bool isNamedWindowInsert,
            TableMetaData insertIntoTable,
            ResultSetProcessorDesc resultSetProcessorDesc,
            PropertyEvaluatorForge propertyEvaluator,
            ExprNode eventPrecedence)
        {
            this.whereClause = whereClause;
            this.isNamedWindowInsert = isNamedWindowInsert;
            this.insertIntoTable = insertIntoTable;
            this.resultSetProcessorDesc = resultSetProcessorDesc;
            this.propertyEvaluator = propertyEvaluator;
            this.eventPrecedence = eventPrecedence;
        }

        public bool IsNamedWindowInsert => isNamedWindowInsert;

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
            method.Block.DeclareVarNewInstance(typeof(OnSplitItemEval), "eval")
                .SetProperty(
                    Ref("eval"),
                    "WhereClause",
                    whereClause == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(whereClause.Forge, method, GetType(), classScope))
                .SetProperty(Ref("eval"), "NamedWindowInsert", Constant(isNamedWindowInsert))
                .SetProperty(
                    Ref("eval"),
                    "InsertIntoTable",
                    insertIntoTable == null
                        ? ConstantNull()
                        : TableDeployTimeResolver.MakeResolveTable(insertIntoTable, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("eval"),
                    "RspFactoryProvider",
                    CodegenExpressionBuilder.NewInstanceInner(resultSetProcessorClassName, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("eval"),
                    "PropertyEvaluator",
                    propertyEvaluator == null ? ConstantNull() : propertyEvaluator.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("eval"),
                    "EventPrecedence",
                    eventPrecedence == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(eventPrecedence.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("eval"));
            return LocalMethod(method);
        }

        public ExprNode WhereClause => whereClause;

        public TableMetaData InsertIntoTable => insertIntoTable;

        public ResultSetProcessorDesc ResultSetProcessorDesc => resultSetProcessorDesc;

        public PropertyEvaluatorForge PropertyEvaluator => propertyEvaluator;

        public string ResultSetProcessorClassName {
            set => resultSetProcessorClassName = value;
        }
    }
} // end of namespace