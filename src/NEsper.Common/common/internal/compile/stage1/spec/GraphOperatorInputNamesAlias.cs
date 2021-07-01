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
    public class GraphOperatorInputNamesAlias
    {
        public GraphOperatorInputNamesAlias(
            string[] inputStreamNames,
            string optionalAsName)
        {
            InputStreamNames = inputStreamNames;
            OptionalAsName = optionalAsName;
        }

        public string[] InputStreamNames { get; private set; }

        public string OptionalAsName { get; private set; }
    }
}