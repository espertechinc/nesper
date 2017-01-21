///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for creating a named window index column.
    /// </summary>
    [Serializable]
    public class CreateIndexItem : MetaDefItem
    {
        public CreateIndexItem(String name, CreateIndexType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; private set; }

        public CreateIndexType Type { get; private set; }
    }
}
