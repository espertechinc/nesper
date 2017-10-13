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
    /// <summary>
    /// Strategy for merging Update properties using all declared property's values.
    /// </summary>
    public class UpdateStrategyDeclared : UpdateStrategyBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="spec">the specification</param>
        public UpdateStrategyDeclared(RevisionSpec spec)
            : base(spec)
        {
        }

        public override void HandleUpdate(bool isBaseEventType,
                                          RevisionStateMerge revisionState,
                                          RevisionEventBeanMerge revisionEvent,
                                          RevisionTypeDesc typesDesc)
        {
            EventBean underlyingEvent = revisionEvent.UnderlyingFullOrDelta;

            // Previously-seen full event
            if (isBaseEventType)
            {
                // If delta types don't add properties, simply set the overlay to null
                NullableObject<Object>[] changeSetValues;
                if (!spec.IsDeltaTypesAddProperties)
                {
                    changeSetValues = null;
                }
                // If delta types do add properties, set a new overlay
                else
                {
                    changeSetValues = revisionState.Overlays;
                    if (changeSetValues == null)
                    {
                        changeSetValues = new NullableObject<Object>[spec.ChangesetPropertyNames.Length];
                    }
                    else
                    {
                        changeSetValues = ArrayCopy(changeSetValues);   // preserve the last revisions
                    }

                    // reset properties not contributed by any delta, leaving all delta-contributed properties in place
                    bool[] changesetPropertyDeltaContributed = spec.ChangesetPropertyDeltaContributed;
                    for (int i = 0; i < changesetPropertyDeltaContributed.Length; i++)
                    {
                        // if contributed then leave the value, else override
                        if (!changesetPropertyDeltaContributed[i])
                        {
                            changeSetValues[i] = null;
                        }
                    }
                }
                revisionState.Overlays = changeSetValues;
                revisionState.BaseEventUnderlying = underlyingEvent;
            }
            // Delta event to existing full event merge
            else
            {
                NullableObject<Object>[] changeSetValues = revisionState.Overlays;

                if (changeSetValues == null)
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
                    Object value = getters[i].Get(underlyingEvent);
                    changeSetValues[index] = new NullableObject<Object>(value);
                }

                revisionState.Overlays = changeSetValues;
            }
        }
    }
}
