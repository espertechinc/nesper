///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    ///     Provides random-access into window contents by event and index as a combination.
    /// </summary>
    public class RelativeAccessByEventNIndexGetterImpl :
        IStreamRelativeAccess.IStreamRelativeAccessUpdateObserver,
        RelativeAccessByEventNIndexGetter,
        PreviousGetterStrategy
    {
        private readonly IDictionary<EventBean, RelativeAccessByEventNIndex> accessorByEvent =
            new Dictionary<EventBean, RelativeAccessByEventNIndex>();

        private readonly IDictionary<RelativeAccessByEventNIndex, EventBean[]> eventsByAccessor =
            new Dictionary<RelativeAccessByEventNIndex, EventBean[]>();

        public void Updated(
            RelativeAccessByEventNIndex iStreamRelativeAccess,
            EventBean[] newData)
        {
            // remove data posted from the last update
            var lastNewData = eventsByAccessor.Get(iStreamRelativeAccess);
            if (lastNewData != null) {
                for (var i = 0; i < lastNewData.Length; i++) {
                    accessorByEvent.Remove(lastNewData[i]);
                }
            }

            if (newData == null) {
                return;
            }

            // hold accessor per event for querying
            for (var i = 0; i < newData.Length; i++) {
                accessorByEvent.Put(newData[i], iStreamRelativeAccess);
            }

            // save new data for access to later removal
            eventsByAccessor.Put(iStreamRelativeAccess, newData);
        }

        public PreviousGetterStrategy GetStrategy(ExprEvaluatorContext ctx)
        {
            return this;
        }

        /// <summary>
        ///     Returns the access into window contents given an event.
        /// </summary>
        /// <param name="theEvent">to which the method returns relative access from</param>
        /// <returns>buffer</returns>
        public RelativeAccessByEventNIndex GetAccessor(EventBean theEvent)
        {
            var iStreamRelativeAccess = accessorByEvent.Get(theEvent);

            return iStreamRelativeAccess;
        }
    }
} // end of namespace