///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.pattern.observer
{
	/// <summary>
	/// Abstract class for applications to extend to implement a pattern observer factory.
	/// </summary>
	public abstract class ObserverFactorySupport : ObserverFactory
	{
        /// <summary>
        /// Sets the observer object parameters.
        /// </summary>
        /// <param name="paramList">is a list of parameters</param>
        /// <param name="convertor">for converting partial pattern matches to event-per-stream for expressions</param>
        /// <param name="validationContext">The validation context.</param>
        /// <throws>ObserverParameterException thrown to indicate a parameter problem</throws>
        public abstract void SetObserverParameters(
	        IList<ExprNode> paramList,
	        MatchedEventConvertor convertor,
	        ExprValidationContext validationContext);

        /// <summary>
        /// Make an observer instance.
        /// </summary>
        /// <param name="context">services that may be required by observer implementation</param>
        /// <param name="beginState">start state for observer</param>
        /// <param name="observerEventEvaluator">receiver for events observed</param>
        /// <param name="stateNodeId">optional id for the associated pattern state node</param>
        /// <param name="observerState">state node for observer</param>
        /// <param name="isFilterChildNonQuitting">if set to <c>true</c> [is filter child non quitting].</param>
        /// <returns>
        /// observer instance
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
	    public abstract EventObserver MakeObserver(
	        PatternAgentInstanceContext context,
	        MatchedEventMap beginState,
	        ObserverEventEvaluator observerEventEvaluator,
	        EvalStateNodeNumber stateNodeId,
	        object observerState,
	        bool isFilterChildNonQuitting);

	    /// <summary>
	    /// Determines whether [is non restarting].
	    /// </summary>
	    /// <value></value>
	    public abstract bool IsNonRestarting { get; }
	}
} // End of namespace
