///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.every
{
    /// <summary>
    ///     This class contains the state of an 'every' operator in the evaluation state tree.
    ///     EVERY nodes work as a factory for new state subnodes. When a child node of an EVERY
    ///     node calls the evaluateTrue method on the EVERY node, the EVERY node will call newState on its child
    ///     node BEFORE it calls evaluateTrue on its parent node. It keeps a reference to the new child in
    ///     its list. (BEFORE because the root node could call quit on child nodes for stopping all
    ///     listeners).
    /// </summary>
    public class EvalEveryStateSpawnEvaluator : Evaluator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EvalEveryStateSpawnEvaluator));

        private readonly string statementName;

        public EvalEveryStateSpawnEvaluator(string statementName)
        {
            this.statementName = statementName;
        }

        public bool IsEvaluatedTrue { get; private set; }

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            log.Warn(
                "Event/request processing: Uncontrolled pattern matching of \"every\" operator - infinite loop when using EVERY operator on expression(s) containing a not operator, for statement '" +
                statementName + "'");
            IsEvaluatedTrue = true;
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            log.Warn(
                "Event/request processing: Uncontrolled pattern matching of \"every\" operator - infinite loop when using EVERY operator on expression(s) containing a not operator, for statement '" +
                statementName + "'");
            IsEvaluatedTrue = true;
        }

        public bool IsFilterChildNonQuitting => true;
    }
} // end of namespace