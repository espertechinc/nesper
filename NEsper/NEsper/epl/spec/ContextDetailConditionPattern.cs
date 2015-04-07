///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailConditionPattern : ContextDetailCondition
    {
        private readonly bool _inclusive;
        private readonly bool _immediate;
    
        [NonSerialized]
        private PatternStreamSpecCompiled _patternCompiled;

        private readonly EvalFactoryNode _patternRaw;

        public ContextDetailConditionPattern(EvalFactoryNode patternRaw, bool inclusive, bool immediate)
        {
            _patternRaw = patternRaw;
            _inclusive = inclusive;
            _immediate = immediate;
        }

        public EvalFactoryNode PatternRaw
        {
            get { return _patternRaw; }
        }

        public PatternStreamSpecCompiled PatternCompiled
        {
            get { return _patternCompiled; }
            set { _patternCompiled = value; }
        }

        public bool IsInclusive
        {
            get { return _inclusive; }
        }

        public bool IsImmediate
        {
            get { return _immediate; }
        }

        public IList<FilterSpecCompiled> FilterSpecIfAny
        {
            get
            {
                var filters = new List<FilterSpecCompiled>();
                var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(_patternCompiled.EvalFactoryNode);
                var filterNodes = evalNodeAnalysisResult.FilterNodes;

                foreach (EvalFilterFactoryNode filterNode in filterNodes)
                {
                    filters.Add(filterNode.FilterSpec);
                }

                return filters.IsEmpty() ? null : filters;
            }
        }
    }
}
