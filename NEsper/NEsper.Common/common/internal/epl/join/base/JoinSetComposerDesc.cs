///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerDesc
    {
        public JoinSetComposerDesc(
            JoinSetComposer joinSetComposer,
            ExprEvaluator postJoinFilterEvaluator)
        {
            JoinSetComposer = joinSetComposer;
            PostJoinFilterEvaluator = postJoinFilterEvaluator;
        }

        public JoinSetComposer JoinSetComposer { get; }

        public ExprEvaluator PostJoinFilterEvaluator { get; }
    }
} // end of namespace