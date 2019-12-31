///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esperio
{
	/// <summary>
	/// An implementation of SendableEvent that wraps a Map event for
	/// sending into the runtime.
	/// </summary>
	public class SendableMapEvent : AbstractSendableEvent
	{
        private readonly DataMap _mapToSend;
		private readonly string _eventTypeName;
		
		/// <summary>
		/// Ctor.
		/// <param name="mapToSend">the map to send into the runtime</param>
		/// <param name="eventTypeName">the event type name for the map event</param>
		/// <param name="timestamp">the timestamp for this event</param>
		/// <param name="scheduleSlot">the schedule slot for the entity that created this event</param>
		/// </summary>
		public SendableMapEvent(DataMap mapToSend, string eventTypeName, long timestamp, long scheduleSlot)
            : base(timestamp, scheduleSlot)
		{
		    //if properties names contain a '.' we need to rebuild the nested map property
		    DataMap toSend = new Dictionary<string,object>();
            foreach (var property in mapToSend.Keys) {
                int dot = property.IndexOf('.');
                if (dot > 0) {
                    string prefix = property.Substring(0, dot);
                    string postfix = property.Substring(dot + 1);
                    if (!toSend.ContainsKey(prefix)) {
                        DataMap nested = new Dictionary<string, object>();
                        nested.Put(postfix, mapToSend.Get(property));
                        toSend.Put(prefix, nested);
                    }
                    else {
                        DataMap nested = (DataMap) toSend.Get(prefix);
                        nested.Put(postfix, mapToSend.Get(property));
                    }
                }
                else {
                    toSend.Put(property, mapToSend.Get(property));
                }
            }

            _mapToSend = toSend;
            _eventTypeName = eventTypeName;
		}

	    #region Overrides of AbstractSendableEvent

        /// <summary> Send the event into the runtime.</summary>
        /// <param name="sender">the sender to send an event</param>
        public override void Send(AbstractSender sender)
	    {
            sender.SendEvent(this, _mapToSend, _eventTypeName);
        }

	    #endregion

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
		public override string ToString()
		{
			return _mapToSend.ToString();
		}
	}
}
