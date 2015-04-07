///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;


namespace com.espertech.esper.multithread
{
    public class MTListener
    {
        private readonly String fieldName;
        private readonly List<object> values;
    
        public MTListener(String fieldName)
        {
            this.fieldName = fieldName;
            values = new List<object>();
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            Object value = e.NewEvents[0].Get(fieldName);
    
            lock(values)
            {
                values.Add(value);
            }
        }

        public List<object> Values
        {
            get { return values; }
        }
    }
}
