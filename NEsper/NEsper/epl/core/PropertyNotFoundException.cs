///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.core
{
	/// <summary> Exception to indicate that a property name used in a filter doesn't resolve.</summary>
    [Serializable]
    public class PropertyNotFoundException : StreamTypesException
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="messageWithoutDetail">The message without detail.</param>
        /// <param name="msgGen">The MSG gen.</param>
        public PropertyNotFoundException(String messageWithoutDetail, StreamTypesExceptionSuggestionGen msgGen)
            : base(messageWithoutDetail, msgGen)
        {
        }
    }
}
