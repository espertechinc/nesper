///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that acts as an adapter between views and Update listeners.
    /// The view can be added to a parent view. When the parent view publishes data, the view will forward the
    /// data to the UpdateListener implementation that has been supplied. If no UpdateListener has been supplied,
    /// then the view will cache the last data published by the parent view.
    /// </summary>

    public sealed class BufferView : ViewSupport
    {
        /// <summary> Set the observer for indicating new and old data.</summary>

        public BufferObserver Observer
        {
            set { _observer = value; }
        }

        private readonly int _streamId;

        private BufferObserver _observer;
        private readonly FlushedEventBuffer _newDataBuffer = new FlushedEventBuffer();
        private readonly FlushedEventBuffer _oldDataBuffer = new FlushedEventBuffer();

        /// <summary> Ctor.</summary>
        /// <param name="streamId">number of the stream for which the view buffers the generated events.
        /// </param>

        public BufferView(int streamId)
        {
            _streamId = streamId;
        }

        /// <summary>
        /// Provides metadata information about the type of object the event collection contains.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// metadata for the objects in the collection
        /// </returns>
        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        /// <summary>
        /// Notify that data has been added or removed from the Viewable parent.
        /// The last object in the newData array of objects would be the newest object added to the parent view.
        /// The first object of the oldData array of objects would be the oldest object removed from the parent view.
        /// <para>
        /// If the call to Update contains new (inserted) data, then the first argument will be a non-empty list and the
        /// second will be empty. Similarly, if the call is a notification of deleted data, then the first argument will be
        /// empty and the second will be non-empty. Either the newData or oldData will be non-null.
        /// This method won't be called with both arguments being null, but either one could be null.
        /// The same is true for zero-length arrays. Either newData or oldData will be non-empty.
        /// If both are non-empty, then the Update is a modification notification.
        /// </para>
        /// 	<para>
        /// When Update() is called on a view by the parent object, the data in newData will be in the collection of the
        /// parent, and its data structures will be arranged to reflect that.
        /// The data in oldData will not be in the parent's data structures, and any access to the parent will indicate that
        /// that data is no longer there.
        /// </para>
        /// </summary>
        /// <param name="newData">is the new data that has been added to the parent view</param>
        /// <param name="oldData">is the old data that has been removed from the parent view</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _newDataBuffer.Add(newData);
            _oldDataBuffer.Add(oldData);
            _observer.NewData(_streamId, _newDataBuffer, _oldDataBuffer);
        }

        /// <summary>Returns the buffer for new data.</summary>
        /// <returns>new data buffer</returns>
        public FlushedEventBuffer NewDataBuffer
        {
            get { return _newDataBuffer; }
        }
    }
}
