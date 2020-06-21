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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    public class OnSplitStreamUtil
    {
        public static OnTriggerPlan HandleSplitStream(
            string aiFactoryProviderClassName,
            CodegenNamespaceScope namespaceScope,
            string classPostfix,
            OnTriggerSplitStreamDesc desc,
            StreamSpecCompiled streamSpec,
            OnTriggerActivatorDesc activatorResult,
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            var raw = @base.StatementSpec.Raw;
            if (raw.InsertIntoDesc == null) {
                throw new ExprValidationException(
                    "Required insert-into clause is not provided, the clause is required for split-stream syntax");
            }

            if (raw.GroupByExpressions != null && raw.GroupByExpressions.Count > 0 ||
                raw.HavingClause != null ||
                raw.OrderByList.Count > 0) {
                throw new ExprValidationException(
                    "A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax");
            }

            var streamName = streamSpec.OptionalStreamName;
            if (streamName == null) {
                streamName = "stream_0";
            }

            StreamTypeService typeServiceTrigger = new StreamTypeServiceImpl(
                new[] {activatorResult.ActivatorResultEventType},
                new[] {streamName},
                new[] {true},
                false,
                false);

            // materialize sub-select views
            SubSelectHelperForgePlan subselectForgePlan = SubSelectHelperForgePlanner.PlanSubSelect(
                @base,
                subselectActivation, 
                new string[]{streamSpec.OptionalStreamName}, 
                new EventType[]{activatorResult.ActivatorResultEventType},
                new string[]{activatorResult.TriggerEventTypeName},
                services);
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges = subselectForgePlan.Subselects;

            // compile top-level split
            var items = new OnSplitItemForge[desc.SplitStreams.Count + 1];
            items[0] = OnSplitValidate(
                typeServiceTrigger,
                @base.StatementSpec,
                @base.ContextPropertyRegistry,
                null,
                @base.StatementRawInfo,
                services);

            // compile each additional split
            var index = 1;
            foreach (var splits in desc.SplitStreams) {
                var splitSpec = new StatementSpecCompiled();

                splitSpec.Raw.InsertIntoDesc = splits.InsertInto;
                splitSpec.SelectClauseCompiled = CompileSelectAllowSubselect(splits.SelectClause);
                splitSpec.Raw.WhereClause = splits.WhereClause;

                PropertyEvaluatorForge optionalPropertyEvaluator = null;
                StreamTypeService typeServiceProperty;
                if (splits.FromClause != null) {
                    optionalPropertyEvaluator = PropertyEvaluatorForgeFactory.MakeEvaluator(
                        splits.FromClause.PropertyEvalSpec,
                        activatorResult.ActivatorResultEventType,
                        streamName,
                        @base.StatementRawInfo,
                        services);
                    typeServiceProperty = new StreamTypeServiceImpl(
                        new[] {optionalPropertyEvaluator.FragmentEventType},
                        new[] {splits.FromClause.OptionalStreamName},
                        new[] {true},
                        false,
                        false);
                }
                else {
                    typeServiceProperty = typeServiceTrigger;
                }

                items[index] = OnSplitValidate(
                    typeServiceProperty,
                    splitSpec,
                    @base.ContextPropertyRegistry,
                    optionalPropertyEvaluator,
                    @base.StatementRawInfo,
                    services);
                index++;
            }

            // handle result set processor classes
            IList<StmtClassForgeable> forgeables = new List<StmtClassForgeable>();
            for (var i = 0; i < items.Length; i++) {
                var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(
                    typeof(ResultSetProcessorFactoryProvider),
                    classPostfix + "_" + i);
                forgeables.Add(
                    new StmtClassForgeableRSPFactoryProvider(
                        classNameRSP,
                        items[i].ResultSetProcessorDesc,
                        namespaceScope,
                        @base.StatementRawInfo));
                items[i].ResultSetProcessorClassName = classNameRSP;
            }

            // plan table access
            var tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(@base.StatementSpec.TableAccessNodes);

            // build forge
            var splitStreamForge = new StatementAgentInstanceFactoryOnTriggerSplitStreamForge(
                activatorResult.Activator,
                activatorResult.ActivatorResultEventType,
                subselectForges,
                tableAccessForges,
                items,
                desc.IsFirst);
            var triggerForge = new StmtClassForgeableAIFactoryProviderOnTrigger(
                aiFactoryProviderClassName,
                namespaceScope,
                splitStreamForge);

            return new OnTriggerPlan(
                triggerForge,
                forgeables,
                new SelectSubscriberDescriptor(),
                subselectForgePlan.AdditionalForgeables);
        }

        private static OnSplitItemForge OnSplitValidate(
            StreamTypeService typeServiceTrigger,
            StatementSpecCompiled statementSpecCompiled,
            ContextPropertyRegistry contextPropertyRegistry,
            PropertyEvaluatorForge optionalPropertyEval,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            var insertIntoName = statementSpecCompiled.Raw.InsertIntoDesc.EventTypeName;
            var isNamedWindowInsert = services.NamedWindowCompileTimeResolver.Resolve(insertIntoName) != null;
            var table = services.TableCompileTimeResolver.Resolve(insertIntoName);
            EPStatementStartMethodHelperValidate.ValidateNodes(
                statementSpecCompiled.Raw,
                typeServiceTrigger,
                null,
                rawInfo,
                services);
            var spec = new ResultSetSpec(statementSpecCompiled);
            var factoryDescs = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                spec,
                typeServiceTrigger,
                null,
                new bool[0],
                false,
                contextPropertyRegistry,
                false,
                true,
                rawInfo,
                services);
            return new OnSplitItemForge(
                statementSpecCompiled.Raw.WhereClause,
                isNamedWindowInsert,
                table,
                factoryDescs,
                optionalPropertyEval);
        }

        /// <summary>
        ///     Compile a select clause allowing subselects.
        /// </summary>
        /// <param name="spec">to compile</param>
        /// <returns>select clause compiled</returns>
        /// <throws>ExprValidationException when validation fails</throws>
        private static SelectClauseSpecCompiled CompileSelectAllowSubselect(SelectClauseSpecRaw spec)
        {
            // Look for expressions with sub-selects in select expression list and filter expression
            // Recursively compile the statement within the statement.
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            IList<SelectClauseElementCompiled> selectElements = new List<SelectClauseElementCompiled>();
            foreach (var raw in spec.SelectExprList) {
                if (raw is SelectClauseExprRawSpec) {
                    var rawExpr = (SelectClauseExprRawSpec) raw;
                    rawExpr.SelectExpression.Accept(visitor);
                    selectElements.Add(
                        new SelectClauseExprCompiledSpec(
                            rawExpr.SelectExpression,
                            rawExpr.OptionalAsName,
                            rawExpr.OptionalAsName,
                            rawExpr.IsEvents));
                }
                else if (raw is SelectClauseStreamRawSpec) {
                    var rawExpr = (SelectClauseStreamRawSpec) raw;
                    selectElements.Add(new SelectClauseStreamCompiledSpec(rawExpr.StreamName, rawExpr.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard) {
                    var wildcard = (SelectClauseElementWildcard) raw;
                    selectElements.Add(wildcard);
                }
                else {
                    throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().Name);
                }
            }

            return new SelectClauseSpecCompiled(selectElements.ToArray(), spec.IsDistinct);
        }
    }
} // end of namespace