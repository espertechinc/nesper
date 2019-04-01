///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.pattern
{
    public class PatternNodeFactoryImpl : PatternNodeFactory
    {
        public EvalFactoryNode MakeAndNode()
        {
            return new EvalAndFactoryNode();
        }

        public EvalFactoryNode MakeEveryDistinctNode(IList<ExprNode> expressions)
        {
            return new EvalEveryDistinctFactoryNode(expressions);
        }

        public EvalFactoryNode MakeEveryNode()
        {
            return new EvalEveryFactoryNode();
        }

        public EvalFactoryNode MakeFilterNode(
            FilterSpecRaw filterSpecification,
            String eventAsName,
            int? consumptionLevel)
        {
            return new EvalFilterFactoryNode(filterSpecification, eventAsName, consumptionLevel);
        }

        public EvalFactoryNode MakeFollowedByNode(IList<ExprNode> maxExpressions, bool hasEngineWideMax)
        {
            return new EvalFollowedByFactoryNode(maxExpressions, hasEngineWideMax);
        }

        public EvalFactoryNode MakeGuardNode(PatternGuardSpec patternGuardSpec)
        {
            return new EvalGuardFactoryNode(patternGuardSpec);
        }

        public EvalFactoryNode MakeMatchUntilNode(ExprNode lowerBounds, ExprNode upperBounds, ExprNode singleBounds)
        {
            return new EvalMatchUntilFactoryNode(lowerBounds, upperBounds, singleBounds);
        }

        public EvalFactoryNode MakeNotNode()
        {
            return new EvalNotFactoryNode();
        }

        public EvalFactoryNode MakeObserverNode(PatternObserverSpec patternObserverSpec)
        {
            return new EvalObserverFactoryNode(patternObserverSpec);
        }

        public EvalFactoryNode MakeOrNode()
        {
            return new EvalOrFactoryNode();
        }

        public EvalRootFactoryNode MakeRootNode(EvalFactoryNode childNode)
        {
            return new EvalRootFactoryNode(childNode);
        }

        public EvalFactoryNode MakeAuditNode(
            bool auditPattern,
            bool auditPatternInstance,
            String expressionText,
            EvalAuditInstanceCount instanceCount,
            bool filterChildNonQuitting)
        {
            return new EvalAuditFactoryNode(
                auditPattern, auditPatternInstance, expressionText, instanceCount, filterChildNonQuitting);
        }

        public bool IsAuditSupported
        {
            get { return true; }
        }
    }
}
