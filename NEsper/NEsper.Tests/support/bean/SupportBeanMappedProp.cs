///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.support.bean
{
    public class SupportBeanMappedProp
    {
        private readonly String id;
        private readonly IDictionary<String, String> mapprop;
    
        public SupportBeanMappedProp(String id, IDictionary<String, String> mapprop)
        {
            this.id = id;
            this.mapprop = mapprop;
        }
    
        public String GetId()
        {
            return id;
        }
    
        public String GetMapEntry(String key)
        {
            return mapprop.Get(key);
        }
    }
}
