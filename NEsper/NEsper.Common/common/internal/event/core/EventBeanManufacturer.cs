///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Factory for creating an event bean instance by writing property values to an
    ///     underlying object.
    /// </summary>
    public interface EventBeanManufacturer
    {
        /// <summary>
        ///     Make an event object populating property values.
        /// </summary>
        /// <param name="properties">values to populate</param>
        /// <returns>
        ///     event object
        /// </returns>
        EventBean Make(object[] properties);

        object MakeUnderlying(object[] properties);
    }

    public class ProxyEventBeanManufacturer : EventBeanManufacturer
    {
        public delegate EventBean MakeFunc(object[] properties);
        public delegate object MakeUnderlyingFunc(object[] properties);

        public MakeFunc ProcMake { get; set; }
        public MakeUnderlyingFunc ProcMakeUnderlying { get; set; }

        public ProxyEventBeanManufacturer(
            MakeFunc procMake,
            MakeUnderlyingFunc procMakeUnderlying)
        {
            ProcMake = procMake;
            ProcMakeUnderlying = procMakeUnderlying;
        }

        public EventBean Make(object[] properties)
        {
            return ProcMake(properties);
        }

        public object MakeUnderlying(object[] properties)
        {
            return ProcMakeUnderlying(properties);
        }
    }
}