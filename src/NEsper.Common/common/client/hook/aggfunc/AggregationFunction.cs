///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.aggfunc
{
    /// <summary>
    /// Maintains aggregation state applying values as entering and leaving the state.
    /// new aggregation state holders and be created from a prototype.
    /// </summary>
    public interface AggregationFunction
    {
        /// <summary>
        /// Apply the value as entering aggregation (entering window).
        /// <para />The value can be null since 'null' values may be counted as unique separate values.
        /// </summary>
        /// <param name="value">to add to aggregate</param>
        void Enter(object value);

        /// <summary>
        /// Apply the value as leaving aggregation (leaving window).
        /// <para />The value can be null since 'null' values may be counted as unique separate values.
        /// </summary>
        /// <param name="value">to remove from aggregate</param>
        void Leave(object value);

        /// <summary>
        /// Returns the current value held.
        /// </summary>
        /// <value>current value</value>
        object Value { get; }

        /// <summary>
        /// Clear out the collection.
        /// </summary>
        void Clear();
    }
} // end of namespace