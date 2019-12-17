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
    public class ValidationException : EPRuntimeException
    {
        public ValidationException(string message)
            : base(message)
        {
        }

        public ValidationException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }
    }
} // end of namespace