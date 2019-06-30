///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodIUDInsertIntoForge : FAFQueryMethodIUDBaseForge
    {
        private SelectExprProcessorForge insertHelper;

        public FAFQueryMethodIUDInsertIntoForge(
            StatementSpecCompiled specCompiled,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
            : base(AssociatedFromClause(specCompiled, services), compilable, statementRawInfo, services)

        {
        }

        protected override void InitExec(
            string aliasName,
            StatementSpecCompiled spec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var selectNoWildcard = InfraOnMergeHelperForge.CompileSelectNoWildcard(
                UuidGenerator.Generate(), Arrays.AsList(spec.SelectClauseCompiled.SelectExprList));

            StreamTypeService streamTypeService = new StreamTypeServiceImpl(true);

            // assign names
            var validationContext = new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services)
                .WithAllowBindingConsumption(true).Build();

            // determine whether column names are provided
            // if the "values" keyword was used, allow sequential automatic name assignment
            string[] assignedSequentialNames = null;
            if (spec.Raw.InsertIntoDesc.ColumnNames.IsEmpty()) {
                var insert = (FireAndForgetSpecInsert) spec.Raw.FireAndForgetSpec;
                if (insert.IsUseValuesKeyword) {
                    assignedSequentialNames = processor.EventTypePublic.PropertyNames;
                }
            }

            var count = -1;
            foreach (var compiled in spec.SelectClauseCompiled.SelectExprList) {
                count++;
                if (compiled is SelectClauseExprCompiledSpec) {
                    var expr = (SelectClauseExprCompiledSpec) compiled;
                    var validatedExpression = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.SELECT, expr.SelectExpression, validationContext);
                    expr.SelectExpression = validatedExpression;
                    if (expr.AssignedName == null) {
                        if (expr.ProvidedName == null) {
                            if (assignedSequentialNames != null && count < assignedSequentialNames.Length) {
                                expr.AssignedName = assignedSequentialNames[count];
                            }
                            else {
                                expr.AssignedName = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression);
                            }
                        }
                        else {
                            expr.AssignedName = expr.ProvidedName;
                        }
                    }
                }
            }

            EventType optionalInsertIntoEventType = processor.EventTypeRspInputEvents;
            var args = new SelectProcessorArgs(
                selectNoWildcard.ToArray(), null,
                false, optionalInsertIntoEventType, null, streamTypeService,
                statementRawInfo.OptionalContextDescriptor,
                true, spec.Annotations, statementRawInfo, services);
            insertHelper = SelectExprProcessorFactory.GetProcessor(args, spec.Raw.InsertIntoDesc, false).Forge;
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
            var anonymousSelect = SelectExprProcessorUtil.MakeAnonymous(insertHelper, method, symbols.GetAddInitSvc(method), classScope);
            method.Block.SetProperty(queryMethod, "InsertHelper", anonymousSelect);
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
                raw.OrderByList != null && !raw.OrderByList.IsEmpty() ||
                raw.RowLimitSpec != null) {
                throw new ExprValidationException("Insert-into fire-and-forget query can only consist of an insert-into clause and a select-clause");
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
                    namedWindow, null, new ViewSpec[0], Collections.GetEmptyList<ExprNode>(), StreamSpecOptions.DEFAULT, null);
            }
            else {
                stream = new TableQueryStreamSpec(null, new ViewSpec[0], StreamSpecOptions.DEFAULT, table, Collections.GetEmptyList<ExprNode>());
            }

            return new StatementSpecCompiled(statementSpec, new[] {stream});
        }
    }
} // end of namespace