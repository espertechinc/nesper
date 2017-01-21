///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableStateViewablePublic : ViewSupport {
    
        private readonly TableMetadata _tableMetadata;
        private readonly TableStateInstance _tableStateInstance;
    
        public TableStateViewablePublic(TableMetadata tableMetadata, TableStateInstance tableStateInstance)
        {
            _tableMetadata = tableMetadata;
            _tableStateInstance = tableStateInstance;
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // no action required
        }

        public override EventType EventType
        {
            get { return _tableMetadata.PublicEventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator() {
            return new TableToPublicEnumerator(_tableStateInstance);
        }

        internal class TableToPublicEnumerator : IEnumerator<EventBean>
        {
            private readonly TableMetadataInternalEventToPublic _eventToPublic;
            private readonly IEnumerator<EventBean> _enumerator;
            private readonly TableStateInstance _tableStateInstance;
    
            internal TableToPublicEnumerator(TableStateInstance tableStateInstance) {
                _eventToPublic = tableStateInstance.TableMetadata.EventToPublic;
                _enumerator = tableStateInstance.EventCollection.GetEnumerator();
                _tableStateInstance = tableStateInstance;
            }
    
            public bool MoveNext() {
                return _enumerator.MoveNext();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public EventBean Current
            {
                get
                {
                    return _eventToPublic.Convert(_enumerator.Current, null, true, _tableStateInstance.AgentInstanceContext);
                }
            }

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}
