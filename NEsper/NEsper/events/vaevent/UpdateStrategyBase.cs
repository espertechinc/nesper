///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>Base strategy implementation holds the specification object. </summary>
    public abstract class UpdateStrategyBase : UpdateStrategy
    {
        /// <summary>The specification. </summary>
        protected readonly RevisionSpec spec;

        /// <summary>Ctor. </summary>
        /// <param name="spec">is the specification</param>
        protected UpdateStrategyBase(RevisionSpec spec)
        {
            this.spec = spec;
        }

        /// <summary>Array copy. </summary>
        /// <param name="array">to copy</param>
        /// <returns>copied array</returns>
        internal static NullableObject<T>[] ArrayCopy<T>(NullableObject<T>[] array)
        {
            if (array == null)
            {
                return null;
            }
            NullableObject<T>[] result = new NullableObject<T>[array.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            return result;
        }

        /// <summary>Merge properties. </summary>
        /// <param name="isBaseEventType">true if the event is a base event type</param>
        /// <param name="revisionState">the current state, to be updated.</param>
        /// <param name="revisionEvent">the new event to merge</param>
        /// <param name="typesDesc">descriptor for event type of the new event to merge</param>
        public abstract void HandleUpdate(bool isBaseEventType,
                                          RevisionStateMerge revisionState,
                                          RevisionEventBeanMerge revisionEvent,
                                          RevisionTypeDesc typesDesc);
    }
}
