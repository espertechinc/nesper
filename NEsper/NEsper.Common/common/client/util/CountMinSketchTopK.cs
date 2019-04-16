///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// ConstantValue object for count-min-sketch top-k.
    /// </summary>
    public class CountMinSketchTopK
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="frequency">the value frequency</param>
        /// <param name="value">the value object</param>
        public CountMinSketchTopK(
            long frequency,
            object value)
        {
            Frequency = frequency;
            Value = value;
        }

        /// <summary>
        /// Returns the frequency
        /// </summary>
        /// <value>frequency</value>
        public long Frequency { get; private set; }

        /// <summary>
        /// Returns the value object
        /// </summary>
        /// <value>value</value>
        public object Value { get; private set; }

        public override string ToString()
        {
            return "CountMinSketchFrequency{" +
                   "frequency=" + Frequency +
                   ", value=" + Value +
                   '}';
        }
    }
}