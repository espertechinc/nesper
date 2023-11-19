///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportBean_A : SupportBeanBase
    {
        [JsonConstructor]
        public SupportBean_A(string id)
            : base(id)
        {
        }
    }
}