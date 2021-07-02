///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.resultset.select.eval;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    /// <summary>
    ///     Factory for select expression processors.
    /// </summary>
    public class SelectExprProcessorFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static SelectExprProcessorDescriptor GetProcessor(
            SelectProcessorArgs args,
            InsertIntoDesc insertIntoDesc,
            bool withSubscriber)
        {
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>();

            SelectExprProcessorWInsertTarget synthetic = GetProcessorInternal(args, insertIntoDesc);
            additionalForgeables.AddAll(synthetic.AdditionalForgeables);

            // plan serdes for variant event types
            if (synthetic.InsertIntoTargetType is VariantEventType ||
                synthetic.InsertIntoTargetType is WrapperEventType && 
                (((WrapperEventType) synthetic.InsertIntoTargetType).UnderlyingEventType is VariantEventType)) {
                var serdeForgeables = SerdeEventTypeUtility.Plan(
                    synthetic.Forge.ResultEventType,
                    args.StatementRawInfo,
                    args.CompileTimeServices.SerdeEventTypeRegistry,
                    args.CompileTimeServices.SerdeResolver);
                additionalForgeables.AddAll(serdeForgeables);
                foreach (EventType eventType in args.TypeService.EventTypes) {
                    serdeForgeables = SerdeEventTypeUtility.Plan(
                        eventType,
                        args.StatementRawInfo,
                        args.CompileTimeServices.SerdeEventTypeRegistry,
                        args.CompileTimeServices.SerdeResolver);
                    additionalForgeables.AddAll(serdeForgeables);
                }
            }

            if (args.IsFireAndForget || !withSubscriber) {
                return new SelectExprProcessorDescriptor(new SelectSubscriberDescriptor(), synthetic.Forge, additionalForgeables);
            }

            // Handle for-clause delivery contract checking
            ExprNode[] groupedDeliveryExpr = null;
            MultiKeyClassRef groupedDeliveryMultiKey = null;

            var forDelivery = false;
            if (args.ForClauseSpec != null) {
                foreach (var item in args.ForClauseSpec.Clauses) {
                    if (item.Keyword == null) {
                        throw new ExprValidationException(
                            "Expected any of the " +
                            EnumHelper.GetValues<ForClauseKeyword>().RenderAny().ToLowerInvariant() +
                            " for-clause keywords after reserved keyword 'for'");
                    }

                    try {
                        var keyword = EnumHelper.Parse<ForClauseKeyword>(item.Keyword);
                        if (keyword == ForClauseKeyword.GROUPED_DELIVERY && item.Expressions.IsEmpty()) {
                            throw new ExprValidationException(
                                "The for-clause with the " +
                                ForClauseKeyword.GROUPED_DELIVERY.GetName() +
                                " keyword requires one or more grouping expressions");
                        }

                        if (keyword == ForClauseKeyword.DISCRETE_DELIVERY && !item.Expressions.IsEmpty()) {
                            throw new ExprValidationException(
                                "The for-clause with the " +
                                ForClauseKeyword.DISCRETE_DELIVERY.GetName() +
                                " keyword does not allow grouping expressions");
                        }

                        if (forDelivery) {
                            throw new ExprValidationException(
                                "The for-clause with delivery keywords may only occur once in a statement");
                        }
                    }
                    catch (ExprValidationException) {
                        throw;
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception) {
                        throw new ExprValidationException(
                            "Expected any of the " +
                            EnumHelper.GetValues<ForClauseKeyword>().RenderAny().ToLowerInvariant() +
                            " for-clause keywords after reserved keyword 'for'");
                    }

                    StreamTypeService type = new StreamTypeServiceImpl(synthetic.Forge.ResultEventType, null, false);
                    groupedDeliveryExpr = new ExprNode[item.Expressions.Count];
                    var validationContext = new ExprValidationContextBuilder(
                            type,
                            args.StatementRawInfo,
                            args.CompileTimeServices)
                        .WithAllowBindingConsumption(true)
                        .Build();
                    for (var i = 0; i < item.Expressions.Count; i++) {
                        groupedDeliveryExpr[i] = ExprNodeUtilityValidate.GetValidatedSubtree(
                            ExprNodeOrigin.FORCLAUSE,
                            item.Expressions[i],
                            validationContext);
                    }

                    forDelivery = true;
                    
                    MultiKeyPlan multiKeyPlan = MultiKeyPlanner.PlanMultiKey(
                        groupedDeliveryExpr, false, args.StatementRawInfo, args.SerdeResolver);
                    groupedDeliveryMultiKey = multiKeyPlan.ClassRef;
                    additionalForgeables = multiKeyPlan.MultiKeyForgeables;
                }

                if (groupedDeliveryExpr != null && groupedDeliveryExpr.Length == 0) {
                    groupedDeliveryExpr = null;
                }
            }

            var allowSubscriber = args.CompileTimeServices.Configuration.Compiler.ByteCode.IsAllowSubscriber;
            SelectSubscriberDescriptor descriptor;
            SelectExprProcessorForge forge;

            if (allowSubscriber) {
                BindProcessorForge bindProcessor = new BindProcessorForge(
                    synthetic.Forge,
                    args.SelectionList,
                    args.TypeService.EventTypes,
                    args.TypeService.StreamNames,
                    args.TableCompileTimeResolver);
                descriptor = new SelectSubscriberDescriptor(
                    bindProcessor.ExpressionTypes,
                    bindProcessor.ColumnNamesAssigned,
                    forDelivery,
                    groupedDeliveryExpr,
                    groupedDeliveryMultiKey);
                forge = new BindSelectExprProcessorForge(synthetic.Forge, bindProcessor);
            } else {
                descriptor = new SelectSubscriberDescriptor();
                forge = new ListenerOnlySelectExprProcessorForge(synthetic.Forge);
            }

            return new SelectExprProcessorDescriptor(descriptor, forge, additionalForgeables);
        }

        private static SelectExprProcessorWInsertTarget GetProcessorInternal(
            SelectProcessorArgs args,
            InsertIntoDesc insertIntoDesc)
        {
            // Wildcard not allowed when insert into specifies column order
            if (args.IsUsingWildcard && insertIntoDesc != null && !insertIntoDesc.ColumnNames.IsEmpty()) {
                throw new ExprValidationException("Wildcard not allowed when insert-into specifies column order");
            }

            var insertIntoTarget = insertIntoDesc == null
                ? null
                : args.EventTypeCompileTimeResolver.GetTypeByName(insertIntoDesc.EventTypeName);

            // Determine wildcard processor (select *)
            if (IsWildcardsOnly(args.SelectionList)) {
                if (args.TypeService.EventTypes.Length == 0) {
                    throw new ExprValidationException("Wildcard cannot be used when the from-clause is not provided");
                }
                // For joins
                if (args.TypeService.StreamNames.Length > 1 && !(insertIntoTarget is VariantEventType)) {
                    Log.Debug(".getProcessor Using SelectExprJoinWildcardProcessor");
                    SelectExprProcessorForgeWForgables pair = SelectExprJoinWildcardProcessorFactory.Create(args, insertIntoDesc, eventTypeName => eventTypeName);
                    SelectExprProcessorForge forgeX = pair.Forge;
                    return new SelectExprProcessorWInsertTarget(forgeX, null, pair.AdditionalForgeables);
                }

                if (insertIntoDesc == null) {
                    // Single-table selects with no insert-into
                    // don't need extra processing
                    Log.Debug(".getProcessor Using wildcard processor");
                    if (args.TypeService.HasTableTypes) {
                        var table = args.TableCompileTimeResolver.ResolveTableFromEventType(
                            args.TypeService.EventTypes[0]);
                        if (table != null) {
                            SelectExprProcessorForge forgeX = new SelectEvalWildcardTable(table);
                            return new SelectExprProcessorWInsertTarget(forgeX, null, EmptyList<StmtClassForgeableFactory>.Instance);
                        }
                    }

                    SelectExprProcessorForge forgeOuter = new SelectEvalWildcardNonJoin(args.TypeService.EventTypes[0]);
                    return new SelectExprProcessorWInsertTarget(forgeOuter, null, EmptyList<StmtClassForgeableFactory>.Instance);
                }
            }

            // Verify the assigned or name used is unique
            if (insertIntoDesc == null) {
                VerifyNameUniqueness(args.SelectionList);
            }

            // Construct processor
            var buckets = GetSelectExpressionBuckets(args.SelectionList);
            var factory = new SelectExprProcessorHelper(
                buckets.Expressions,
                buckets.SelectedStreams,
                args,
                insertIntoDesc);
            return factory.Forge;
        }

        protected internal static void VerifyNameUniqueness(SelectClauseElementCompiled[] selectionList)
        {
            ISet<string> names = new HashSet<string>();
            foreach (var element in selectionList) {
                if (element is SelectClauseExprCompiledSpec) {
                    var expr = (SelectClauseExprCompiledSpec) element;
                    if (names.Contains(expr.AssignedName)) {
                        throw new ExprValidationException(
                            "Column name '" + expr.AssignedName + "' appears more then once in select clause");
                    }

                    names.Add(expr.AssignedName);
                }
                else if (element is SelectClauseStreamCompiledSpec) {
                    var stream = (SelectClauseStreamCompiledSpec) element;
                    if (stream.OptionalName == null) {
                        continue; // ignore no-name stream selectors
                    }

                    if (names.Contains(stream.OptionalName)) {
                        throw new ExprValidationException(
                            "Column name '" + stream.OptionalName + "' appears more then once in select clause");
                    }

                    names.Add(stream.OptionalName);
                }
            }
        }

        private static bool IsWildcardsOnly(SelectClauseElementCompiled[] elements)
        {
            foreach (var element in elements) {
                if (!(element is SelectClauseElementWildcard)) {
                    return false;
                }
            }

            return true;
        }

        private static SelectExprBuckets GetSelectExpressionBuckets(SelectClauseElementCompiled[] elements)
        {
            IList<SelectClauseExprCompiledSpec> expressions = new List<SelectClauseExprCompiledSpec>();
            IList<SelectExprStreamDesc> selectedStreams = new List<SelectExprStreamDesc>();

            foreach (var element in elements) {
                if (element is SelectClauseExprCompiledSpec) {
                    var expr = (SelectClauseExprCompiledSpec) element;
                    if (!IsTransposingFunction(expr.SelectExpression)) {
                        expressions.Add(expr);
                    }
                    else {
                        selectedStreams.Add(new SelectExprStreamDesc(expr));
                    }
                }
                else if (element is SelectClauseStreamCompiledSpec) {
                    selectedStreams.Add(new SelectExprStreamDesc((SelectClauseStreamCompiledSpec) element));
                }
            }

            return new SelectExprBuckets(expressions, selectedStreams);
        }

        private static bool IsTransposingFunction(ExprNode selectExpression)
        {
            if (!(selectExpression is ExprDotNode)) {
                return false;
            }

            var dotNode = (ExprDotNode) selectExpression;
            var chainSpec = dotNode.ChainSpec;
            if (dotNode.ChainSpec.IsEmpty()) {
                return false;
            }
            var first = chainSpec[0];
            return ImportServiceCompileTimeConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE.Equals(first.GetRootNameOrEmptyString().ToLowerInvariant());
        }

        public class SelectExprBuckets
        {
            public SelectExprBuckets(
                IList<SelectClauseExprCompiledSpec> expressions,
                IList<SelectExprStreamDesc> selectedStreams)
            {
                Expressions = expressions;
                SelectedStreams = selectedStreams;
            }

            public IList<SelectExprStreamDesc> SelectedStreams { get; }

            public IList<SelectClauseExprCompiledSpec> Expressions { get; }
        }
    }
} // end of namespace