///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.filter
{
    /// <summary>
    ///     This class represents a filter of events in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFilterForgeNode : EvalForgeNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized] private FilterSpecCompiled filterSpec;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="attachPatternText">whether to attach EPL subexpression text</param>
        /// <param name="filterSpecification">specifies the filter properties</param>
        /// <param name="eventAsName">
        ///     is the name to use for adding matching events to the MatchedEventMaptable used when indicating truth value of true.
        /// </param>
        /// <param name="consumptionLevel">when using @consume</param>
        public EvalFilterForgeNode(
            bool attachPatternText,
            FilterSpecRaw filterSpecification,
            string eventAsName,
            int? consumptionLevel)
            : base(attachPatternText)
        {
            RawFilterSpec = filterSpecification;
            EventAsName = eventAsName;
            ConsumptionLevel = consumptionLevel;
        }

        /// <summary>
        ///     Returns the raw (unoptimized/validated) filter definition.
        /// </summary>
        /// <returns>filter def</returns>
        public FilterSpecRaw RawFilterSpec { get; }

        /// <summary>
        ///     Returns filter specification.
        /// </summary>
        /// <returns>filter definition</returns>
        public FilterSpecCompiled FilterSpecCompiled => filterSpec;

        /// <summary>
        ///     Returns the tag for any matching events to this filter, or null since tags are optional.
        /// </summary>
        /// <returns>tag string for event</returns>
        public string EventAsName { get; }

        public int? ConsumptionLevel { get; }

        public bool IsFilterChildNonQuitting => false;

        public int EventAsTagNumber { get; set; } = -1;

        public bool IsStateful => false;

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.ATOM;

        /// <summary>
        ///     Sets a validated and optimized filter specification
        /// </summary>
        /// <value>is the optimized filter</value>
        public FilterSpecCompiled FilterSpec {
            get => filterSpec;
            set => filterSpec = value;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("EvalFilterNode rawFilterSpec=" + RawFilterSpec);
            buffer.Append(" filterSpec=" + filterSpec);
            buffer.Append(" eventAsName=" + EventAsName);
            return buffer.ToString();
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (EventAsName != null) {
                writer.Write(EventAsName);
                writer.Write("=");
            }

            writer.Write(RawFilterSpec.EventTypeName);
            if (RawFilterSpec.FilterExpressions != null && RawFilterSpec.FilterExpressions.Count > 0) {
                writer.Write("(");
                ExprNodeUtilityPrint.ToExpressionStringParameterList(RawFilterSpec.FilterExpressions, writer);
                writer.Write(")");
            }

            if (ConsumptionLevel != null) {
                writer.Write("@consume");
                if (ConsumptionLevel != 1) {
                    writer.Write("(");
                    writer.Write(Convert.ToString(ConsumptionLevel));
                    writer.Write(")");
                }
            }
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalFilterFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "Filter";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    Ref("node"),
                    "FilterSpec",
                    LocalMethod(filterSpec.MakeCodegen(method, symbols, classScope)))
                .SetProperty(Ref("node"), "EventAsName", Constant(EventAsName))
                .SetProperty(Ref("node"), "ConsumptionLevel", Constant(ConsumptionLevel))
                .SetProperty(Ref("node"), "EventAsTagNumber", Constant(EventAsTagNumber));
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
            filters.Add(filterSpec);
        }

        protected override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.PATTERN_FILTER;
        }
    }
} // end of namespace