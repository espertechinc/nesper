///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class MTListener : UpdateListener
    {
        private readonly string fieldName;

        public MTListener(string fieldName)
        {
            this.fieldName = fieldName;
            Values = new List<object>();
        }

        public IList<object> Values { get; }

        public void Update(
            object sender,
            UpdateEventArgs eventArgs)
        {
            var value = eventArgs.NewEvents[0].Get(fieldName);

            lock (Values) {
                Values.Add(value);
            }
        }
    }
} // end of namespace