///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.@base
{
    public class JoinSetComposerDesc
    {
        public JoinSetComposerDesc(JoinSetComposer joinSetComposer, ExprEvaluator postJoinFilterEvaluator)
        {
            JoinSetComposer = joinSetComposer;
            PostJoinFilterEvaluator = postJoinFilterEvaluator;
        }

        public JoinSetComposer JoinSetComposer { get; private set; }

        public ExprEvaluator PostJoinFilterEvaluator { get; private set; }
    }
}
