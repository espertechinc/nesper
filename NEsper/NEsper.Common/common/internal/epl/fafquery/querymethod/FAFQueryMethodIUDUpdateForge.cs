///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodIUDUpdateForge : FAFQueryMethodIUDBaseForge
    {
        public const string INITIAL_VALUE_STREAM_NAME = "initial";

        private EventBeanUpdateHelperForge updateHelper;

        public FAFQueryMethodIUDUpdateForge(
            StatementSpecCompiled spec,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
            : base(spec, compilable, statementRawInfo, services)

        {
        }

        protected override void InitExec(
            string aliasName,
            StatementSpecCompiled spec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            StreamTypeServiceImpl assignmentTypeService = new StreamTypeServiceImpl(
                new EventType[] {
                    processor.EventTypeRspInputEvents, null,
                    processor.EventTypeRspInputEvents
                },
                new string[] {aliasName, "", INITIAL_VALUE_STREAM_NAME},
                new bool[] {true, true, true}, true, false);
            assignmentTypeService.IsStreamZeroUnambigous = true;
            ExprValidationContext validationContext = new ExprValidationContextBuilder(assignmentTypeService, statementRawInfo, services)
                .WithAllowBindingConsumption(true).Build();

            // validate update expressions
            FireAndForgetSpecUpdate updateSpec = (FireAndForgetSpecUpdate) spec.Raw.FireAndForgetSpec;
            try {
                foreach (OnTriggerSetAssignment assignment in updateSpec.Assignments) {
                    ExprNode validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.UPDATEASSIGN, assignment.Expression, validationContext);
                    assignment.Expression = validated;
                    EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                        validated, "Aggregation functions may not be used within an update-clause");
                }
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }

            // make updater
            //TableUpdateStrategy tableUpdateStrategy = null;
            try {
                bool copyOnWrite = processor is FireAndForgetProcessorNamedWindowForge;
                updateHelper = EventBeanUpdateHelperForgeFactory.Make(
                    processor.NamedWindowOrTableName,
                    (EventTypeSPI) processor.EventTypeRspInputEvents, updateSpec.Assignments, aliasName, null, copyOnWrite,
                    statementRawInfo.StatementName, services.EventTypeAvroHandler);
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
        }

        protected override Type TypeOfMethod()
        {
            return typeof(FAFQueryMethodIUDUpdate);
        }

        protected override void MakeInlineSpecificSetter(
            CodegenExpressionRef queryMethod,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                queryMethod, "setOptionalWhereClause",
                whereClause == null
                    ? ConstantNull()
                    : ExprNodeUtilityCodegen.CodegenEvaluator(whereClause.Forge, method, this.GetType(), classScope));
            if (processor is FireAndForgetProcessorNamedWindowForge) {
                method.Block.ExprDotMethod(queryMethod, "setUpdateHelperNamedWindow", updateHelper.MakeWCopy(method, classScope));
            }
            else {
                FireAndForgetProcessorTableForge table = (FireAndForgetProcessorTableForge) processor;
                method.Block
                    .ExprDotMethod(queryMethod, "setUpdateHelperTable", updateHelper.MakeNoCopy(method, classScope))
                    .ExprDotMethod(queryMethod, "setTable", TableDeployTimeResolver.MakeResolveTable(table.Table, symbols.GetAddInitSvc(method)));
            }
        }
    }
} // end of namespace