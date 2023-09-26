///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodIUDInsertIntoForge : FAFQueryMethodIUDBaseForge
    {
        private const int MAX_MULTIROW = 1000;

        private SelectExprProcessorForge[] insertHelpers;

        public FAFQueryMethodIUDInsertIntoForge(
            StatementSpecCompiled specCompiled,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services) : base(
            AssociatedFromClause(specCompiled, services),
            compilable,
            statementRawInfo,
            services)
        {
        }

        protected override void InitExec(
            string aliasName,
            StatementSpecCompiled spec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(true);

            // assign names
            var validationContext = new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services)
                .WithAllowBindingConsumption(true)
                .Build();

            // determine whether column names are provided
            // if the "values" keyword was used, allow sequential automatic name assignment
            string[] assignedSequentialNames = null;
            if (spec.Raw.InsertIntoDesc.ColumnNames.IsEmpty()) {
                var insert = (FireAndForgetSpecInsert)spec.Raw.FireAndForgetSpec;
                if (insert.IsUseValuesKeyword) {
                    assignedSequentialNames = processor.EventTypePublic.PropertyNames;
                }
            }

            if (spec.Raw.InsertIntoDesc.EventPrecedence != null) {
                throw new ExprValidationException("Fire-and-forget insert-queries do not allow event-precedence");
            }

            var insertSpec = (FireAndForgetSpecInsert)spec.Raw.FireAndForgetSpec;
            if (insertSpec.Multirow.IsEmpty()) {
                var selectNoWildcard = InfraOnMergeHelperForge.CompileSelectNoWildcard(
                    UuidGenerator.Generate(),
                    Arrays.AsList(spec.SelectClauseCompiled.SelectExprList));
                var insert = InitExecRow(
                    selectNoWildcard,
                    spec,
                    assignedSequentialNames,
                    validationContext,
                    streamTypeService,
                    statementRawInfo,
                    services);
                insertHelpers = new SelectExprProcessorForge[] { insert };
            }
            else {
                var numRows = insertSpec.Multirow.Count;
                if (numRows > MAX_MULTIROW) {
                    throw new ExprValidationException(
                        "Insert-into number-of-rows exceeds the maximum of " +
                        MAX_MULTIROW +
                        " rows as the query provides " +
                        numRows +
                        " rows");
                }

                var count = 0;
                insertHelpers = new SelectExprProcessorForge[insertSpec.Multirow.Count];
                foreach (var row in insertSpec.Multirow) {
                    IList<SelectClauseElementCompiled> selected = new List<SelectClauseElementCompiled>(row.Count);
                    foreach (var expr in row) {
                        selected.Add(new SelectClauseExprCompiledSpec(expr, null, null, false));
                    }

                    try {
                        insertHelpers[count++] = InitExecRow(
                            selected,
                            spec,
                            assignedSequentialNames,
                            validationContext,
                            streamTypeService,
                            statementRawInfo,
                            services);
                    }
                    catch (ExprValidationException ex) {
                        if (insertSpec.Multirow.Count == 1) {
                            throw;
                        }

                        throw new ExprValidationException(
                            "Failed to validate multi-row insert at row " +
                            count +
                            " of " +
                            insertHelpers.Length +
                            ": " +
                            ex.Message,
                            ex);
                    }
                }
            }
        }

        private SelectExprProcessorForge InitExecRow(
            IList<SelectClauseElementCompiled> select,
            StatementSpecCompiled spec,
            string[] assignedSequentialNames,
            ExprValidationContext validationContext,
            StreamTypeService streamTypeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var count = -1;
            foreach (var compiled in select) {
                count++;
                if (compiled is SelectClauseExprCompiledSpec expr) {
                    var validatedExpression = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.SELECT,
                        expr.SelectExpression,
                        validationContext);
                    expr.SelectExpression = validatedExpression;
                    if (expr.AssignedName == null) {
                        if (expr.ProvidedName == null) {
                            if (assignedSequentialNames != null && count < assignedSequentialNames.Length) {
                                expr.AssignedName = assignedSequentialNames[count];
                            }
                            else {
                                expr.AssignedName =
                                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression);
                            }
                        }
                        else {
                            expr.AssignedName = expr.ProvidedName;
                        }
                    }
                }
            }

            var selected = select.ToArray();

            var optionalInsertIntoEventType = processor.EventTypeRSPInputEvents;
            var args = new SelectProcessorArgs(
                selected,
                null,
                false,
                optionalInsertIntoEventType,
                null,
                streamTypeService,
                statementRawInfo.OptionalContextDescriptor,
                true,
                spec.Annotations,
                statementRawInfo,
                services);
            return SelectExprProcessorFactory.GetProcessor(args, spec.Raw.InsertIntoDesc, false).Forge;
        }

        protected override Type TypeOfMethod()
        {
            return typeof(FAFQueryMethodIUDInsertInto);
        }

        protected override void MakeInlineSpecificSetter(
            CodegenExpressionRef queryMethod,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.DeclareVar<SelectExprProcessor[]>(
                "helpers",
                NewArrayByLength(typeof(SelectExprProcessor), Constant(insertHelpers.Length)));
            
            for (var i = 0; i < insertHelpers.Length; i++) {
                var select = SelectExprProcessorUtil.MakeAnonymous(
                    insertHelpers[i],
                    method,
                    symbols.GetAddInitSvc(method),
                    classScope);
                method.Block.AssignArrayElement("helpers", Constant(i), select);
            }

            method.Block.SetProperty(queryMethod, "InsertHelpers", Ref("helpers"));
        }

        private static StatementSpecCompiled AssociatedFromClause(
            StatementSpecCompiled statementSpec,
            StatementCompileTimeServices services)
        {
            var raw = statementSpec.Raw;
            if (raw.WhereClause != null ||
                statementSpec.StreamSpecs.Length > 0 ||
                raw.HavingClause != null ||
                raw.OutputLimitSpec != null ||
                raw.ForClauseSpec != null ||
                raw.MatchRecognizeSpec != null ||
                (raw.OrderByList != null && !raw.OrderByList.IsEmpty()) ||
                raw.RowLimitSpec != null) {
                throw new ExprValidationException(
                    "Insert-into fire-and-forget query can only consist of an insert-into clause and a select-clause");
            }

            var infraName = statementSpec.Raw.InsertIntoDesc.EventTypeName;
            var namedWindow = services.NamedWindowCompileTimeResolver.Resolve(infraName);
            var table = services.TableCompileTimeResolver.Resolve(infraName);
            if (namedWindow == null && table == null) {
                throw new ExprValidationException("Failed to find named window or table '" + infraName + "'");
            }

            StreamSpecCompiled stream;
            if (namedWindow != null) {
                stream = new NamedWindowConsumerStreamSpec(
                    namedWindow,
                    null,
                    new ViewSpec[0],
                    EmptyList<ExprNode>.Instance,
                    StreamSpecOptions.DEFAULT,
                    null);
            }
            else {
                stream = new TableQueryStreamSpec(
                    null,
                    new ViewSpec[0],
                    StreamSpecOptions.DEFAULT,
                    table,
                    EmptyList<ExprNode>.Instance);
            }

            return new StatementSpecCompiled(statementSpec, new StreamSpecCompiled[] { stream });
        }
    }
} // end of namespace