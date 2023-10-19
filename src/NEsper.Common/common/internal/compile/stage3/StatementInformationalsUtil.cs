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

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StatementInformationalsUtil
    {
        public const string EPL_ONSTART_SCRIPT_NAME = "on_statement_start";
        public const string EPL_ONSTOP_SCRIPT_NAME = "on_statement_stop";
        public const string EPL_ONLISTENERUPDATE_SCRIPT_NAME = "on_statement_listener_update";

        public static StatementInformationalsCompileTime GetInformationals(
            StatementBaseInfo @base,
            IList<FilterSpecTracked> filterSpecCompileds,
            IList<ScheduleHandleTracked> schedules,
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers,
            bool allowContext,
            SelectSubscriberDescriptor selectSubscriberDescriptor,
            CodegenNamespaceScope namespaceScope,
            StatementCompileTimeServices services)
        {
            var specCompiled = @base.StatementSpec;

            var alwaysSynthesizeOutputEvents =
                specCompiled.Raw.InsertIntoDesc != null ||
                specCompiled.Raw.ForClauseSpec != null ||
                specCompiled.SelectClauseCompiled.IsDistinct ||
                specCompiled.Raw.CreateDataFlowDesc != null;
            var needDedup = IsNeedDedup(filterSpecCompileds);
            var hasSubquery = !@base.StatementSpec.SubselectNodes.IsEmpty();
            var canSelfJoin = StatementSpecWalkUtil.IsPotentialSelfJoin(specCompiled) || needDedup;

            // Determine stateless statement
            var stateless = DetermineStatelessSelect(
                @base.StatementRawInfo.StatementType,
                @base.StatementSpec.Raw,
                !@base.StatementSpec.SubselectNodes.IsEmpty());

            string contextName = null;
            string contextModuleName = null;
            NameAccessModifier? contextVisibility = null;
            if (allowContext) {
                var descriptor = @base.StatementRawInfo.OptionalContextDescriptor;
                if (descriptor != null) {
                    contextName = descriptor.ContextName;
                    contextModuleName = descriptor.ContextModuleName;
                    contextVisibility = descriptor.ContextVisibility;
                }
            }

            var annotationData = AnnotationAnalysisResult.AnalyzeAnnotations(@base.StatementSpec.Annotations);
            // Hint annotations are often driven by variables
            var hasHint = false;
            if (@base.StatementSpec.Raw.Annotations != null) {
                foreach (var annotation in @base.StatementRawInfo.Annotations) {
                    if (annotation is HintAttribute) {
                        hasHint = true;
                    }
                }
            }

            var hasVariables = hasHint ||
                               !@base.StatementSpec.Raw.ReferencedVariables.IsEmpty() ||
                               @base.StatementSpec.Raw.CreateContextDesc != null;
            var writesToTables = StatementLifecycleSvcUtil.IsWritesToTables(
                @base.StatementSpec.Raw,
                services.TableCompileTimeResolver);
            var hasTableAccess = StatementLifecycleSvcUtil.DetermineHasTableAccess(
                @base.StatementSpec.SubselectNodes,
                @base.StatementSpec.Raw,
                services.TableCompileTimeResolver);

            IDictionary<StatementProperty, object> properties = new Dictionary<StatementProperty, object>();
            if (services.Configuration.Compiler.ByteCode.IsAttachEPL) {
                properties.Put(StatementProperty.EPL, @base.Compilable.ToEPL());
            }

            string insertIntoLatchName = null;
            if (@base.StatementSpec.Raw.InsertIntoDesc != null ||
                @base.StatementSpec.Raw.OnTriggerDesc is OnTriggerMergeDesc) {
                if (@base.StatementSpec.Raw.InsertIntoDesc != null) {
                    insertIntoLatchName = @base.StatementSpec.Raw.InsertIntoDesc.EventTypeName;
                }
                else {
                    insertIntoLatchName = "merge";
                }
            }

            var allowSubscriber = services.Configuration.Compiler.ByteCode.IsAllowSubscriber;

            var statementScripts = @base.StatementSpec.Raw.ScriptExpressions;
            IList<ExpressionScriptProvided> onScripts = new List<ExpressionScriptProvided>();
            if (statementScripts != null) {
                foreach (var script in statementScripts) {
                    if (script.Name.Equals(EPL_ONLISTENERUPDATE_SCRIPT_NAME) ||
                        script.Name.Equals(EPL_ONSTART_SCRIPT_NAME) ||
                        script.Name.Equals(EPL_ONSTOP_SCRIPT_NAME)) {
                        onScripts.Add(script);
                    }
                }
            }

            return new StatementInformationalsCompileTime(
                services.Container,
                @base.StatementName,
                alwaysSynthesizeOutputEvents,
                contextName,
                contextModuleName,
                contextVisibility,
                canSelfJoin,
                hasSubquery,
                needDedup,
                specCompiled.Annotations,
                stateless,
                @base.UserObjectCompileTime,
                filterSpecCompileds.Count,
                schedules.Count,
                namedWindowConsumers.Count,
                @base.StatementRawInfo.StatementType,
                annotationData.Priority,
                annotationData.IsPremptive,
                hasVariables,
                writesToTables,
                hasTableAccess,
                selectSubscriberDescriptor.SelectClauseTypes,
                selectSubscriberDescriptor.SelectClauseColumnNames,
                selectSubscriberDescriptor.IsForClauseDelivery,
                selectSubscriberDescriptor.GroupDelivery,
                selectSubscriberDescriptor.GroupDeliveryMultiKey,
                properties,
                @base.StatementSpec.Raw.MatchRecognizeSpec != null,
                services.IsInstrumented,
                namespaceScope,
                insertIntoLatchName,
                allowSubscriber,
                onScripts.ToArray());
        }

        private static bool IsNeedDedup(IList<FilterSpecTracked> filterSpecCompileds)
        {
            foreach (var provider in filterSpecCompileds) {
                if (provider.FilterSpecCompiled.Parameters.Paths.Length > 1) {
                    return true;
                }
            }

            return false;
        }

        internal static bool DetermineStatelessSelect(
            StatementType type,
            StatementSpecRaw spec,
            bool hasSubselects)
        {
            if (hasSubselects) {
                return false;
            }

            if (type != StatementType.SELECT) {
                return false;
            }

            if (spec.StreamSpecs == null || spec.StreamSpecs.Count > 1 || spec.StreamSpecs.IsEmpty()) {
                return false;
            }

            var singleStream = spec.StreamSpecs[0];
            if (!(singleStream is FilterStreamSpecRaw) && !(singleStream is NamedWindowConsumerStreamSpec)) {
                return false;
            }

            if (singleStream.ViewSpecs != null && singleStream.ViewSpecs.Length > 0) {
                return false;
            }

            if (spec.OutputLimitSpec != null) {
                return false;
            }

            if (spec.MatchRecognizeSpec != null) {
                return false;
            }

            var expressions = StatementSpecRawWalkerExpr.CollectExpressionsShallow(spec);
            if (expressions.IsEmpty()) {
                return true;
            }

            var visitor = new ExprNodeSummaryVisitor();
            foreach (var expr in expressions) {
                expr?.Accept(visitor);
            }

            return !visitor.HasAggregation && !visitor.HasPreviousPrior && !visitor.HasSubselect;
        }

        /// <summary>
        ///     Analysis result of analysing annotations for a statement.
        /// </summary>
        public class AnnotationAnalysisResult
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="priority">priority</param>
            /// <param name="premptive">preemptive indicator</param>
            private AnnotationAnalysisResult(
                int priority,
                bool premptive)
            {
                Priority = priority;
                IsPremptive = premptive;
            }

            /// <summary>
            ///     Returns execution priority.
            /// </summary>
            /// <returns>priority.</returns>
            public int Priority { get; }

            /// <summary>
            ///     Returns preemptive indicator (drop or normal).
            /// </summary>
            /// <value>true for drop</value>
            public bool IsPremptive { get; }

            /// <summary>
            ///     Analyze the annotations and return priority and drop settings.
            /// </summary>
            /// <param name="annotations">to analyze</param>
            /// <returns>analysis result</returns>
            public static AnnotationAnalysisResult AnalyzeAnnotations(Attribute[] annotations)
            {
                var preemptive = false;
                var priority = 0;
                var hasPrioritySetting = false;
                foreach (var annotation in annotations) {
                    if (annotation is PriorityAttribute priorityAttribute) {
                        priority = priorityAttribute.Value;
                        hasPrioritySetting = true;
                    }

                    if (annotation is DropAttribute) {
                        preemptive = true;
                    }
                }

                if (!hasPrioritySetting && preemptive) {
                    priority = 1;
                }

                return new AnnotationAnalysisResult(priority, preemptive);
            }
        }
    }
} // end of namespace