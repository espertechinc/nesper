///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// For use with Count-min sketch aggregation functions:
    /// The agent implementation encapsulates transformation of value objects to byte-array and back (when needed),
    /// and may override or provide custom behavior.
    /// </summary>
    public interface CountMinSketchAgent
    {
        /// <summary>
        /// Returns an array of types that the agent can handle, for validation purposes.
        /// For example, an agent that accepts byte-array type values should return "new Class[] {typeof(String)}".
        /// Interfaces and supertype classes can also be part of the class array.
        /// </summary>
        /// <value>class array of acceptable type</value>
        Type[] AcceptableValueTypes { get; }

        /// <summary>
        /// Add a value to the Count-min sketch.
        /// Implementations typically check for null value, convert the value object to a byte-array
        /// and invoke a method on the state object to add the byte-array value.
        /// </summary>
        /// <param name="ctx">contains value to add as well as the state</param>
        void Add(CountMinSketchAgentContextAdd ctx);

        /// <summary>
        /// Return the estimated count for a given value.
        /// Implementations typically check for null value, convert the value object to a byte-array
        /// and invoke a method on the state object to retrieve a count.
        /// </summary>
        /// <param name="ctx">contains value to query as well as the state</param>
        /// <returns>estimated count</returns>
        long? Estimate(CountMinSketchAgentContextEstimate ctx);

        /// <summary>
        /// Return the value object for a given byte-array, for use with top-K.
        /// Implementations typically simply convert a byte-array into a value object.
        /// </summary>
        /// <param name="ctx">value object and state</param>
        /// <returns>value object</returns>
        object FromBytes(CountMinSketchAgentContextFromBytes ctx);
    }
}