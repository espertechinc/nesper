///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class contains the state of an 'every' operator in the evaluation state tree.
    /// EVERY nodes work as a factory for new state subnodes. When a child node of an 
    /// EVERY node calls the evaluateTrue method on the EVERY node, the EVERY node will call 
    /// newState on its child node BEFORE it calls evaluateTrue on its parent node. It keeps 
    /// a reference to the new child in its list. (BEFORE because the root node could call 
    /// quit on child nodes for stopping all listeners).
    /// </summary>
    public sealed class EvalEveryStateSpawnEvaluator : Evaluator
    {
        private bool _isEvaluatedTrue;

        private readonly String _statementName;

        public EvalEveryStateSpawnEvaluator(String statementName)
        {
            _statementName = statementName;
        }

        public bool IsEvaluatedTrue
        {
            get { return _isEvaluatedTrue; }
        }

        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            Log.Warn("Event/request processing: Uncontrolled pattern matching of \"every\" operator - infinite loop when using EVERY operator on Expression(s) containing a not operator, for statement '" + _statementName + "'");
            _isEvaluatedTrue = true;
        }

        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            Log.Warn("Event/request processing: Uncontrolled pattern matching of \"every\" operator - infinite loop when using EVERY operator on Expression(s) containing a not operator, for statement '" + _statementName + "'");
            _isEvaluatedTrue = true;
        }

        public bool IsFilterChildNonQuitting
        {
            get { return true; }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
