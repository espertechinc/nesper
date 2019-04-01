///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class CreateContextDesc
    {
        public CreateContextDesc(string contextName, ContextSpec contextDetail)
        {
            ContextName = contextName;
            ContextDetail = contextDetail;
        }

        public string ContextName { get; }

        public ContextSpec ContextDetail { get; }
    }
} // end of namespace