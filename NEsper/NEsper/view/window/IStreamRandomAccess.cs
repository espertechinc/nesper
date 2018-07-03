///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// For use with length and time window views that must provide random access into 
    /// data window contents provided for the "previous" expression if used.
    /// </summary>
    public class IStreamRandomAccess : RandomAccessByIndex, ViewUpdatedCollection
    {
        private readonly List<EventBean> _arrayList;
        private readonly RandomAccessByIndexObserver _updateObserver;
    
        /// <summary>Ctor. </summary>
        /// <param name="updateObserver">is invoked when updates are received</param>
        public IStreamRandomAccess(RandomAccessByIndexObserver updateObserver)
        {
            _updateObserver = updateObserver;
            _arrayList = new List<EventBean>();
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (_updateObserver !=null)
            {
                _updateObserver.Updated(this);
            }
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    _arrayList.Insert(0, newData[i]);
                }
            }
    
            if (oldData != null)
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    _arrayList.RemoveAt(_arrayList.Count - 1);
                }
            }
        }
    
        /// <summary>Remove event. </summary>
        /// <param name="oldData">event to remove</param>
        public void Remove(EventBean oldData)
        {
            if (_updateObserver !=null)
            {
                _updateObserver.Updated(this);
            }
            _arrayList.RemoveAt(_arrayList.Count - 1);
        }
    
        /// <summary>Apply event </summary>
        /// <param name="newData">to apply</param>
        public void Update(EventBean newData)
        {
            if (_updateObserver !=null)
            {
                _updateObserver.Updated(this);
            }
            _arrayList.Insert(0, newData);
        }
    
        public EventBean GetNewData(int index)
        {
            // New events are added to the start of the list
            if (index < _arrayList.Count )
            {
                return _arrayList[index];
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
    
        /// <summary>Returns true for empty. </summary>
        /// <returns>indicator</returns>
        public bool IsEmpty()
        {
            return _arrayList.IsEmpty();
        }
    
        public EventBean GetNewDataTail(int index)
        {
            // New events are added to the start of the list
            if (index < _arrayList.Count && index >= 0)
            {
                return _arrayList[_arrayList.Count - index - 1];
            }
            return null;
        }
    
        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            return _arrayList.GetEnumerator();
        }

        public ICollection<EventBean> WindowCollectionReadOnly => _arrayList;

        public int WindowCount => _arrayList.Count;

        public int NumEventsInsertBuf => WindowCount;
    }
}
