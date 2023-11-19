///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Context detail for a library-func and filter pair for the hash segmented context. </summary>
    public class ContextDescriptorHashSegmentedItem : ContextDescriptor
    {
        /// <summary>Ctor. </summary>
        public ContextDescriptorHashSegmentedItem()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="hashFunction">the hash function, expecting SingleRowMethodExpression</param>
        /// <param name="filter">the event types to apply to</param>
        public ContextDescriptorHashSegmentedItem(
            Expression hashFunction,
            Filter filter)
        {
            HashFunction = hashFunction;
            Filter = filter;
        }

        /// <summary>Returns the filter. </summary>
        /// <value>filter</value>
        public Filter Filter { get; set; }

        /// <summary>Returns the hash function. </summary>
        /// <value>hash function</value>
        public Expression HashFunction { get; set; }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            HashFunction?.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

            writer.Write(" from ");
            Filter.ToEPL(writer, formatter);
        }
    }
}