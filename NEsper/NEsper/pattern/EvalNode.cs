///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Superclass of all nodes in an evaluation tree representing an event pattern expression. 
    /// Follows the Composite pattern. Child nodes do not carry references to parent nodes, the 
    /// tree is unidirectional.
    /// </summary>
    public interface EvalNode
    {
        EvalStateNode NewState(Evaluator parentNode,
                                      EvalStateNodeNumber stateNodeNumber,
                                      long stateNodeId);
    }
}
