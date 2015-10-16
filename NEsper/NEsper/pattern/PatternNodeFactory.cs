///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.pattern
{
    public interface PatternNodeFactory
    {
        EvalFactoryNode MakeAndNode();
        EvalFactoryNode MakeEveryDistinctNode(IList<ExprNode> expressions);
        EvalFactoryNode MakeEveryNode();
        EvalFactoryNode MakeFilterNode(FilterSpecRaw filterSpecification,String eventAsName, int? consumptionLevel);
        EvalFactoryNode MakeFollowedByNode(IList<ExprNode> maxExpressions, bool hasEngineWideMax);
        EvalFactoryNode MakeGuardNode(PatternGuardSpec patternGuardSpec);
        EvalFactoryNode MakeMatchUntilNode(ExprNode lowerBounds, ExprNode upperBounds, ExprNode singleBounds);
        EvalFactoryNode MakeNotNode();
        EvalFactoryNode MakeObserverNode(PatternObserverSpec patternObserverSpec);
        EvalFactoryNode MakeOrNode();
        EvalRootFactoryNode MakeRootNode(EvalFactoryNode childNode);
    }
}
