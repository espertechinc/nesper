///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    /// <summary>
    ///     All current state holding partial NFA matches.
    /// </summary>
    public class RowRecogPartitionStateImpl : RowRecogPartitionState
    {
        private IList<RowRecogNFAStateEntry> _currentStatesInternal = new List<RowRecogNFAStateEntry>();

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="randomAccess">for handling "prev" functions, if any</param>
        /// <param name="optionalKeys">keys for "partition", if any</param>
        public RowRecogPartitionStateImpl(
            RowRecogStateRandomAccess randomAccess,
            object optionalKeys)
        {
            RandomAccess = randomAccess;
            OptionalKeys = optionalKeys;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">for "prev" access</param>
        /// <param name="currentStates">existing state</param>
        public RowRecogPartitionStateImpl(
            RowRecogPreviousStrategyImpl getter,
            IList<RowRecogNFAStateEntry> currentStates)
            : this(getter, currentStates, null)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">for "prev" access</param>
        /// <param name="currentStates">existing state</param>
        /// <param name="optionalKeys">partition keys if any</param>
        public RowRecogPartitionStateImpl(
            RowRecogPreviousStrategyImpl getter,
            IList<RowRecogNFAStateEntry> currentStates,
            object optionalKeys)
        {
            if (getter != null) {
                RandomAccess = new RowRecogStateRandomAccessImpl(getter);
            }

            CurrentStatesForPrint = currentStates;
            OptionalKeys = optionalKeys;
        }

        /// <summary>
        ///     Returns the random access for "prev".
        /// </summary>
        /// <returns>access</returns>
        public RowRecogStateRandomAccess RandomAccess { get; }

        /// <summary>
        ///     Returns partial matches.
        /// </summary>
        /// <returns>state</returns>
        public IEnumerator<RowRecogNFAStateEntry> CurrentStatesIterator => CurrentStatesForPrint.GetEnumerator();

        /// <summary>
        ///     Sets partial matches.
        /// </summary>
        /// <value>state to set</value>
        public IList<RowRecogNFAStateEntry> CurrentStates {
            get => _currentStatesInternal;
            set => _currentStatesInternal = value;
        }

        /// <summary>
        ///     Returns partition keys, if any.
        /// </summary>
        /// <returns>keys</returns>
        public object OptionalKeys { get; }

        public int NumStates => CurrentStatesForPrint.Count;

        public IList<RowRecogNFAStateEntry> CurrentStatesForPrint {
            get => _currentStatesInternal;
            private set => _currentStatesInternal = value;
        }

        public bool IsEmptyCurrentState => CurrentStatesForPrint.IsEmpty();

        /// <summary>
        ///     Remove an event from random access for "prev".
        /// </summary>
        /// <param name="oldEvents">to remove</param>
        public void RemoveEventFromPrev(EventBean[] oldEvents)
        {
            RandomAccess?.Remove(oldEvents);
        }

        /// <summary>
        ///     Remove an event from random access for "prev".
        /// </summary>
        /// <param name="oldEvent">to remove</param>
        public void RemoveEventFromPrev(EventBean oldEvent)
        {
            RandomAccess?.Remove(oldEvent);
        }

        /// <summary>
        ///     Remove an event from state.
        /// </summary>
        /// <param name="oldEvent">to remove</param>
        /// <returns>true for removed, false for not found</returns>
        public int RemoveEventFromState(EventBean oldEvent)
        {
            var currentSize = CurrentStatesForPrint.Count;
            var keepList = RemoveEventFromState(oldEvent, CurrentStatesForPrint.GetEnumerator());
            RandomAccess?.Remove(oldEvent);

            CurrentStatesForPrint = keepList;
            return currentSize - keepList.Count;
        }

        public void ClearCurrentStates()
        {
            CurrentStatesForPrint.Clear();
        }

        public static IList<RowRecogNFAStateEntry> RemoveEventFromState(
            EventBean oldEvent,
            IEnumerator<RowRecogNFAStateEntry> states)
        {
            IList<RowRecogNFAStateEntry> keepList = new List<RowRecogNFAStateEntry>();
            for (; states.MoveNext();) {
                var entry = states.Current;
                var keep = true;

                var state = entry.EventsPerStream;
                foreach (var aState in state) {
                    if (aState != null && aState.Equals(oldEvent)) {
                        keep = false;
                        break;
                    }
                }

                if (keep) {
                    var multimatch = entry.OptionalMultiMatches;
                    if (multimatch != null) {
                        foreach (var aMultimatch in multimatch) {
                            if (aMultimatch != null && aMultimatch.ContainsEvent(oldEvent)) {
                                keep = false;
                                break;
                            }
                        }
                    }
                }

                if (keep) {
                    keepList.Add(entry);
                }
            }

            return keepList;
        }
    }
} // end of namespace