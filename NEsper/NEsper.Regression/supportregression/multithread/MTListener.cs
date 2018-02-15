///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.supportregression.multithread
{
    public class MTListener
    {
        private readonly string _fieldName;
        private readonly IList<object> _values;
    
        public MTListener(string fieldName) {
            _fieldName = fieldName;
            _values = new List<object>();
        }

        public void Update(object sender, UpdateEventArgs e)
        {
            var value = e.NewEvents[0].Get(_fieldName);
    
            lock (_values) {
                _values.Add(value);
            }
        }

        public IList<object> Values => _values;
    }
} // end of namespace
