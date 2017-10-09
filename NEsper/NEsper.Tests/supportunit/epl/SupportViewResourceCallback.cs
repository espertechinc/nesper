///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportViewResourceCallback
    {
        private readonly IList<Object> _resources = new List<Object>();
    
        public void SetViewResource(Object resource)
        {
            _resources.Add(_resources);
        }

        public IList<object> Resources
        {
            get { return _resources; }
        }
    }
}
