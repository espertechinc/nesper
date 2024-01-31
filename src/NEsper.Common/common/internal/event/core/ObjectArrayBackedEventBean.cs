///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     For events that are array of properties.
    /// </summary>
    public interface ObjectArrayBackedEventBean : EventBean
    {
        /// <summary>Returns property array. </summary>
        /// <value>properties</value>
        object[] Properties { get; }

        object[] PropertyValues { set; }
    }
}