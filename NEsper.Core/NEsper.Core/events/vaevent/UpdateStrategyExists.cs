///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>Strategy for merging Update properties using only existing property's values. </summary>
    public class UpdateStrategyExists : UpdateStrategyBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="spec">the specification</param>
        public UpdateStrategyExists(RevisionSpec spec)
            : base(spec)
        {
        }

        public override void HandleUpdate(bool isBaseEventType,
                                          RevisionStateMerge revisionState,
                                          RevisionEventBeanMerge revisionEvent,
                                          RevisionTypeDesc typesDesc)
        {
            EventBean underlyingEvent = revisionEvent.UnderlyingFullOrDelta;

            NullableObject<Object>[] changeSetValues = revisionState.Overlays;
            if (changeSetValues == null)    // optimization - the full event sets it to null, deltas all get a new one
            {
                changeSetValues = new NullableObject<Object>[spec.ChangesetPropertyNames.Length];
            }
            else
            {
                changeSetValues = ArrayCopy(changeSetValues);   // preserve the last revisions
            }

            // apply all properties of the delta event
            int[] indexes = typesDesc.ChangesetPropertyIndex;
            EventPropertyGetter[] getters = typesDesc.ChangesetPropertyGetters;
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];

                if (!getters[i].IsExistsProperty(underlyingEvent))
                {
                    continue;
                }

                Object value = getters[i].Get(underlyingEvent);
                changeSetValues[index] = new NullableObject<Object>(value);
            }

            revisionState.Overlays = changeSetValues;
        }
    }
}
