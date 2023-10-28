///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.table.ExprTableAccessNode.AccessEvaluationType;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public abstract class ExprTableAccessNode : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator
    {
        private readonly string tableName;
        private TableMetaData tableMeta;
        private ExprTableEvalStrategyFactoryForge strategy;
        private ExprForge[] groupKeyEvaluators;
        private int tableAccessNumber = -1;

        protected ExprForge[] GroupKeyEvaluators => groupKeyEvaluators;

        [JsonIgnore]
        public abstract Type EvaluationType { get; }
        
        protected abstract void ValidateBindingInternal(ExprValidationContext validationContext);

        protected abstract bool EqualsNodeInternal(ExprTableAccessNode other);

        [JsonIgnore]
        protected abstract string InstrumentationQName { get; }

        [JsonIgnore]
        protected abstract CodegenExpression[] InstrumentationQParams { get; }

        [JsonIgnore]
        public abstract ExprTableEvalStrategyFactoryForge TableAccessFactoryForge { get; }

        /// <summary>
        /// Ctor.
        /// Getting a table name allows "eplToModel" without knowing tables.
        /// </summary>
        /// <param name="tableName">table name</param>
        public ExprTableAccessNode(string tableName)
        {
            this.tableName = tableName;
        }

        public string TableName => tableName;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            tableMeta = validationContext.TableCompileTimeResolver.Resolve(tableName);
            if (tableMeta == null) {
                throw new ExprValidationException("Failed to resolve table name '" + tableName + "' to a table");
            }

            if (!validationContext.IsAllowBindingConsumption) {
                throw new ExprValidationException(
                    "Invalid use of table access expression, expression '" + TableName + "' is not allowed here");
            }

            if (tableMeta.OptionalContextName != null &&
                validationContext.ContextDescriptor != null &&
                !tableMeta.OptionalContextName.Equals(validationContext.ContextDescriptor.ContextName)) {
                throw new ExprValidationException(
                    "Table by name '" +
                    TableName +
                    "' has been declared for context '" +
                    tableMeta.OptionalContextName +
                    "' and can only be used within the same context");
            }

            // additional validations depend on detail
            ValidateBindingInternal(validationContext);
            return null;
        }

        protected void ValidateGroupKeys(
            TableMetaData metadata,
            ExprValidationContext validationContext)
        {
            if (ChildNodes.Length > 0) {
                groupKeyEvaluators = ExprNodeUtilityQuery.GetForges(ChildNodes);
            }
            else {
                groupKeyEvaluators = Array.Empty<ExprForge>();
            }

            var typesReturned = ExprNodeUtilityQuery.GetExprResultTypes(ChildNodes);
            var keyTypes = metadata.IsKeyed ? metadata.KeyTypes : Type.EmptyTypes;
            ExprTableNodeUtil.ValidateExpressions(TableName, typesReturned, "key", ChildNodes, keyTypes, "key");
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        [JsonIgnore]
        public ExprNodeRenderable ForgeRenderable => this;
        
        [JsonIgnore]
        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        
        [JsonIgnore]
        public ExprNodeRenderable EnumForgeRenderable { get; }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(PLAIN, this, EvaluationType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    InstrumentationQName,
                    requiredType,
                    parent,
                    exprSymbol,
                    codegenClassScope)
                .Qparams(InstrumentationQParams)
                .Build();
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                GETEVENTCOLL,
                this,
                typeof(ICollection<EventBean>),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                GETSCALARCOLL,
                this,
                typeof(ICollection<object>),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(GETEVENT, this, typeof(EventBean), parent, exprSymbol, codegenClassScope);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public virtual ExprEvaluator ExprEvaluator => this;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        protected void ToPrecedenceFreeEPLInternal(
            TextWriter writer,
            string subprop,
            ExprNodeRenderableFlags flags)
        {
            ToPrecedenceFreeEPLInternal(writer, flags);
            writer.Write(".");
            writer.Write(subprop);
        }

        protected void ToPrecedenceFreeEPLInternal(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(TableName);
            if (ChildNodes.Length > 0) {
                writer.Write("[");
                var delimiter = "";
                foreach (var expr in ChildNodes) {
                    writer.Write(delimiter);
                    expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
                    delimiter = ",";
                }

                writer.Write("]");
            }
        }

        protected TableMetadataColumn ValidateSubpropertyGetCol(
            TableMetaData tableMetadata,
            string subpropName)
        {
            var column = tableMetadata.Columns.Get(subpropName);
            if (column == null) {
                throw new ExprValidationException(
                    "A column '" + subpropName + "' could not be found for table '" + TableName + "'");
            }

            return column;
        }

        public override bool EqualsNode(
            ExprNode o,
            bool ignoreStreamPrefix)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprTableAccessNode)o;
            if (!TableName.Equals(that.TableName)) {
                return false;
            }

            return EqualsNodeInternal(that);
        }

        public int TableAccessNumber {
            get => tableAccessNumber;
            set => tableAccessNumber = value;
        }

        public TableMetaData TableMeta => tableMeta;

        internal static CodegenExpression MakeEvaluate(
            AccessEvaluationType evaluationType,
            ExprTableAccessNode accessNode,
            Type resultType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (resultType == null) {
                return ConstantNull();
            }

            var resultClass = resultType;
            if (accessNode.TableAccessNumber == -1) {
                throw new IllegalStateException("Table expression node has not been assigned");
            }

            var method = parent.MakeChild(resultClass, typeof(ExprTableAccessNode), classScope);

            CodegenExpression eps = symbols.GetAddEPS(method);
            var newData = symbols.GetAddIsNewData(method);
            CodegenExpression evalCtx = symbols.GetAddExprEvalCtx(method);

            var future = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameTableAccess(accessNode.tableAccessNumber),
                typeof(ExprTableEvalStrategy));
            var evaluation = ExprDotMethod(future, evaluationType.MethodName, eps, newData, evalCtx);
            if (resultClass != typeof(object)) {
                evaluation = CodegenLegoCast.CastSafeFromObjectType(resultType, evaluation);
                //evaluation = Cast(resultType, evaluation);
            }

            method.Block.MethodReturn(evaluation);
            return LocalMethod(method);
        }

        public class AccessEvaluationType
        {
            public static readonly AccessEvaluationType PLAIN = new AccessEvaluationType("Evaluate");

            public static readonly AccessEvaluationType GETEVENTCOLL =
                new AccessEvaluationType("EvaluateGetROCollectionEvents");

            public static readonly AccessEvaluationType GETSCALARCOLL =
                new AccessEvaluationType("EvaluateGetROCollectionScalar");

            public static readonly AccessEvaluationType GETEVENT = new AccessEvaluationType("EvaluateGetEventBean");

            public static readonly AccessEvaluationType EVALTYPABLESINGLE =
                new AccessEvaluationType("EvaluateTypableSingle");

            private AccessEvaluationType(string methodName)
            {
                MethodName = methodName;
            }

            public string MethodName { get; }
        }
    }
} // end of namespace