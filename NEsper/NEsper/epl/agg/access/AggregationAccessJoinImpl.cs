///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationAccessJoinImpl : AggregationAccess
    {
        private readonly LinkedHashMap<EventBean, int> refSet = new LinkedHashMap<EventBean, int>();
        private readonly int streamId;
        private EventBean[] _array;

        /// <summary>Ctor. </summary>
        /// <param name="streamId">stream id</param>
        public AggregationAccessJoinImpl(int streamId)
        {
            this.streamId = streamId;
        }

        #region AggregationAccess Members

        public virtual void Clear()
        {
            refSet.Clear();
            _array = null;
        }

        public void ApplyEnter(EventBean[] eventsPerStream)
        {
            EventBean theEvent = eventsPerStream[streamId];
            if (theEvent == null)
            {
                return;
            }
            _array = null;

            int value;
            if (!refSet.TryGetValue(theEvent, out value))
            {
                refSet.Put(theEvent, 1);
                return;
            }

            value++;
            refSet.Put(theEvent, value);
        }

        public void ApplyLeave(EventBean[] eventsPerStream)
        {
            EventBean theEvent = eventsPerStream[streamId];
            if (theEvent == null)
            {
                return;
            }
            _array = null;

            int value;
            if (!refSet.TryGetValue(theEvent, out value))
            {
                return;
            }

            if (value == 1)
            {
                refSet.Remove(theEvent);
                return;
            }

            value--;
            refSet.Put(theEvent, value);
        }

        public EventBean GetFirstNthValue(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (refSet.IsEmpty())
            {
                return null;
            }
            if (index >= refSet.Count)
            {
                return null;
            }
            if (_array == null)
            {
                InitArray();
            }
            return _array[index];
        }

        public EventBean GetLastNthValue(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (refSet.IsEmpty())
            {
                return null;
            }
            if (index >= refSet.Count)
            {
                return null;
            }
            if (_array == null)
            {
                InitArray();
            }
            return _array[_array.Length - index - 1];
        }

        public EventBean FirstValue
        {
            get
            {
                if (refSet.IsEmpty())
                {
                    return null;
                }
                return refSet.Keys.First();
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (refSet.IsEmpty())
                {
                    return null;
                }
                if (_array == null)
                {
                    InitArray();
                }
                return _array[_array.Length - 1];
            }
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (_array == null)
            {
                InitArray();
            }
            return ((IEnumerable<EventBean>) _array).GetEnumerator();
        }

        public ICollection<EventBean> CollectionReadOnly()
        {
            if (_array == null)
            {
                InitArray();
            }
            return _array;
        }

        public int Count
        {
            get { return refSet.Count; }
        }

        #endregion

        private void InitArray()
        {
            ICollection<EventBean> events = refSet.Keys;
            _array = events.ToArray();
        }
    }
}