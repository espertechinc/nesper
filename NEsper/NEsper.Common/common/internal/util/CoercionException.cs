///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Exception to represent a Mismatch in types in an expression.
    /// </summary>
    [Serializable]
    public class CoercionException : EPException
    {
        /// <summary> Ctor.</summary>
        /// <param name="message">supplies the detailed description
        /// </param>
        public CoercionException(string message)
            : base(message)
        {
        }
    }
}