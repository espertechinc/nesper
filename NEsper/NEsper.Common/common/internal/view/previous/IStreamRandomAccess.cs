///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.previous
{
    /// <summary>
    /// For use with length and time window views that must provide random access into data window contents
    /// provided for the "previous" expression if used.
    /// </summary>
    public class IStreamRandomAccess : RandomAccessByIndex,
        ViewUpdatedCollection
    {
        private readonly List<EventBean> arrayList;
        private readonly RandomAccessByIndexObserver updateObserver;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="updateObserver">is invoked when updates are received</param>
        public IStreamRandomAccess(RandomAccessByIndexObserver updateObserver)
        {
            this.updateObserver = updateObserver;
            this.arrayList = new List<EventBean>();
        }

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            updateObserver?.Updated(this);
            if (newData != null) {
                for (int i = 0; i < newData.Length; i++) {
                    arrayList.Insert(0, newData[i]);
                }
            }

            if (oldData != null) {
                for (int i = 0; i < oldData.Length; i++) {
                    arrayList.RemoveAt(arrayList.Count - 1);
                }
            }
        }

        /// <summary>
        /// Remove event.
        /// </summary>
        /// <param name="oldData">event to remove</param>
        public void Remove(EventBean oldData)
        {
            if (updateObserver != null) {
                updateObserver.Updated(this);
            }

            arrayList.RemoveAt(arrayList.Count - 1);
        }

        /// <summary>
        /// Apply event
        /// </summary>
        /// <param name="newData">to apply</param>
        public void Update(EventBean newData)
        {
            if (updateObserver != null) {
                updateObserver.Updated(this);
            }

            arrayList.Insert(0, newData);
        }

        public EventBean GetNewData(int index)
        {
            // New events are added to the start of the list
            if (index < arrayList.Count) {
                return arrayList[index];
            }

            return null;
        }

        public EventBean GetOldData(int index)
        {
            return null;
        }

        public void Destroy()
        {
            // No action required
        }

        /// <summary>
        /// Returns true for empty.
        /// </summary>
        /// <returns>indicator</returns>
        public bool IsEmpty {
            get => arrayList.IsEmpty();
        }

        public EventBean GetNewDataTail(int index)
        {
            // New events are added to the start of the list
            if (index < arrayList.Count && index >= 0) {
                return arrayList[arrayList.Count - index - 1];
            }

            return null;
        }

        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            return arrayList.GetEnumerator();
        }

        public ICollection<EventBean> WindowCollectionReadOnly {
            get => arrayList;
        }

        public int WindowCount {
            get => arrayList.Count;
        }

        public int NumEventsInsertBuf {
            get => WindowCount;
        }
    }
} // end of namespace