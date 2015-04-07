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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailInitiatedTerminated : ContextDetail
    {
        private ContextDetailCondition _start;
        private ContextDetailCondition _end;
        private readonly bool _overlapping;
        private readonly ExprNode[] _distinctExpressions;

        public ContextDetailInitiatedTerminated(ContextDetailCondition start, ContextDetailCondition end, bool overlapping, ExprNode[] distinctExpressions)
        {
            _start = start;
            _end = end;
            _overlapping = overlapping;
            _distinctExpressions = distinctExpressions;
        }

        public ContextDetailCondition Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public ContextDetailCondition End
        {
            get { return _end; }
            set { _end = value; }
        }

        public bool IsOverlapping
        {
            get { return _overlapping; }
        }

        public IList<FilterSpecCompiled> FilterSpecsIfAny
        {
            get
            {
                IList<FilterSpecCompiled> startFS = _start.FilterSpecIfAny;
                IList<FilterSpecCompiled> endFS = _end.FilterSpecIfAny;
                if (startFS == null && endFS == null)
                {
                    return null;
                }
                IList<FilterSpecCompiled> filters = new List<FilterSpecCompiled>(2);
                if (startFS != null)
                {
                    filters.AddAll(startFS);
                }
                if (endFS != null)
                {
                    filters.AddAll(endFS);
                }
                return filters;
            }
        }

        public ExprNode[] DistinctExpressions
        {
            get { return _distinctExpressions; }
        }
    }
}
