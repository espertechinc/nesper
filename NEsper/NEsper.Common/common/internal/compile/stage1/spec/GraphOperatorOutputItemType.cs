///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class GraphOperatorOutputItemType
    {
        public GraphOperatorOutputItemType(
            bool wildcard,
            String typeOrClassname,
            IList<GraphOperatorOutputItemType> typeParameters)
        {
            IsWildcard = wildcard;
            TypeOrClassname = typeOrClassname;
            TypeParameters = typeParameters;
        }

        public bool IsWildcard { get; private set; }

        public string TypeOrClassname { get; private set; }

        public IList<GraphOperatorOutputItemType> TypeParameters { get; private set; }
    }
}