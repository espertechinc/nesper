///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.context
{
    /// <summary>Indicates an invalid combination of context declaration and context partition selector, i.e. cageory context with hash context partition selector. </summary>
    public class InvalidContextPartitionSelector : EPException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">exception message</param>
        public InvalidContextPartitionSelector(string message)
            : base(message)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="message">exception message</param>
        /// <param name="cause">inner exception</param>
        public InvalidContextPartitionSelector(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="cause">inner exception</param>
        public InvalidContextPartitionSelector(Exception cause)
            : base(cause)
        {
        }
    }
}