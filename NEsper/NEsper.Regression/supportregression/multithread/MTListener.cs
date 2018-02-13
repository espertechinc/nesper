///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class MTListener : UpdateListener {
        private readonly string _fieldName;
        private List _values;
    
        public MTListener(string fieldName) {
            this._fieldName = fieldName;
            _values = new LinkedList();
        }
    
        public void Update(EventBean[] newEvents, EventBean[] oldEvents) {
            var value = newEvents[0].Get(_fieldName);
    
            lock (_values) {
                _values.Add(value);
            }
        }
    
        public List GetValues() {
            return _values;
        }
    }
} // end of namespace
