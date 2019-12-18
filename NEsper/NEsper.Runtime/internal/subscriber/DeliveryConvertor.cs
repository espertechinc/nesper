///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>Converts a row of column selection results into a result for dispatch to a method. </summary>
    public interface DeliveryConvertor
    {
        /// <summary>Convert result row to dispatchable. </summary>
        /// <param name="row">to convert</param>
        /// <returns>converted row</returns>
        Object[] ConvertRow(Object[] row);
    }
}