///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class GraphOperatorDetail
    {
        public GraphOperatorDetail(IDictionary<String, Object> configs)
        {
            Configs = configs;
        }

        public IDictionary<string, object> Configs { get; private set; }
    }
}