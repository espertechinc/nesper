///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    public class ResultDeliveryStrategyInvalidException : Exception
    {
        public ResultDeliveryStrategyInvalidException(string message) : base(message)
        {
        }
    }
} // end of namespace
