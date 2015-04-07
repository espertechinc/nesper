///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a filter of events in the evaluation tree representing any event expressions.
    /// </summary>
    [Serializable]
    public class EvalFilterFactoryNode : EvalNodeFactoryBase
    {
        private readonly FilterSpecRaw _rawFilterSpec;
        private readonly String _eventAsName;
        [NonSerialized] private FilterSpecCompiled _filterSpec;
        private readonly int? _consumptionLevel;
    
        private int _eventAsTagNumber = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filterSpecification">specifies the filter properties</param>
        /// <param name="eventAsName">is the name to use for adding matching events to the MatchedEventMaptable used when indicating truth value of true.</param>
        /// <param name="consumptionLevel">The consumption level.</param>
        public EvalFilterFactoryNode(FilterSpecRaw filterSpecification,
                                     String eventAsName,
                                     int? consumptionLevel)
        {
            _rawFilterSpec = filterSpecification;
            _eventAsName = eventAsName;
            _consumptionLevel = consumptionLevel;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext)
        {
            return new EvalFilterNode(agentInstanceContext, this);
        }

        /// <summary>Returns the raw (unoptimized/validated) filter definition. </summary>
        /// <value>filter def</value>
        public FilterSpecRaw RawFilterSpec
        {
            get { return _rawFilterSpec; }
        }

        /// <summary>Returns filter specification. </summary>
        /// <value>filter definition</value>
        public FilterSpecCompiled FilterSpec
        {
            get { return _filterSpec; }
            set { _filterSpec = value; }
        }

        /// <summary>Returns the tag for any matching events to this filter, or null since tags are optional. </summary>
        /// <value>tag string for event</value>
        public string EventAsName
        {
            get { return _eventAsName; }
        }

        public int? ConsumptionLevel
        {
            get { return _consumptionLevel; }
        }

        public override String ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("EvalFilterNode rawFilterSpec=" + _rawFilterSpec);
            buffer.Append(" filterSpec=" + _filterSpec);
            buffer.Append(" eventAsName=" + _eventAsName);
            return buffer.ToString();
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public int EventAsTagNumber
        {
            get { return _eventAsTagNumber; }
            set { _eventAsTagNumber = value; }
        }

        public override bool IsStateful
        {
            get { return false; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            if (EventAsName != null) {
                writer.Write(EventAsName);
                writer.Write("=");
            }
            writer.Write(_rawFilterSpec.EventTypeName);
            if (_rawFilterSpec.FilterExpressions != null && _rawFilterSpec.FilterExpressions.Count > 0) {
                writer.Write("(");
                ExprNodeUtility.ToExpressionStringParameterList(_rawFilterSpec.FilterExpressions, writer);
                writer.Write(")");
            }
            if (_consumptionLevel != null) {
                writer.Write("@consume");
                if (_consumptionLevel != 1) {
                    writer.Write("(");
                    writer.Write(Convert.ToString(_consumptionLevel));
                    writer.Write(")");
                }
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.ATOM; }
        }
    }
}
