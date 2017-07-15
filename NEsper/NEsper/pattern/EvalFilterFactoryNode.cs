///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;
using System.Text;

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a filter of events in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFilterFactoryNode : EvalNodeFactoryBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized] private FilterSpecCompiled _filterSpec;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filterSpecification">specifies the filter properties</param>
        /// <param name="eventAsName">
        /// is the name to use for adding matching events to the MatchedEventMap
        /// table used when indicating truth value of true.
        /// </param>
        /// <param name="consumptionLevel">when using @consume</param>
        protected EvalFilterFactoryNode(FilterSpecRaw filterSpecification,
                                        string eventAsName,
                                        int? consumptionLevel)
        {
            EventAsTagNumber = -1;
            RawFilterSpec = filterSpecification;
            EventAsName = eventAsName;
            ConsumptionLevel = consumptionLevel;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            return new EvalFilterNode(agentInstanceContext, this);
        }

        /// <summary>
        /// Returns the raw (unoptimized/validated) filter definition.
        /// </summary>
        /// <value>filter def</value>
        public FilterSpecRaw RawFilterSpec { get; private set; }

        /// <summary>
        /// Returns filter specification.
        /// </summary>
        /// <value>filter definition</value>
        public FilterSpecCompiled FilterSpec
        {
            get { return _filterSpec; }
            set { _filterSpec = value; }
        }

        /// <summary>
        /// Returns the tag for any matching events to this filter, or null since tags are optional.
        /// </summary>
        /// <value>tag string for event</value>
        public string EventAsName { get; private set; }

        public int? ConsumptionLevel { get; private set; }

        public override String ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("EvalFilterNode rawFilterSpec=" + RawFilterSpec);
            buffer.Append(" filterSpec=" + _filterSpec);
            buffer.Append(" eventAsName=" + EventAsName);
            return buffer.ToString();
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public int EventAsTagNumber { get; set; }

        public override bool IsStateful
        {
            get { return false; }
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
                ExprNodeUtility.ToExpressionStringParameterList(RawFilterSpec.FilterExpressions, writer);
                writer.Write(")");
            }
            if (ConsumptionLevel != null) {
                writer.Write("@consume");
                if (ConsumptionLevel != 1) {
                    writer.Write("(");
                    writer.Write(ConsumptionLevel);
                    writer.Write(")");
                }
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.ATOM; }
        }
    }
} // end of namespace
