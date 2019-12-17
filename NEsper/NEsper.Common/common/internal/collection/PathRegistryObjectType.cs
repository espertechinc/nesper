///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.collection
{
    public class PathRegistryObjectType
    {
        public static readonly PathRegistryObjectType CONTEXT =
            new PathRegistryObjectType("context", "A");

        public static readonly PathRegistryObjectType NAMEDWINDOW =
            new PathRegistryObjectType("named window", "A");

        public static readonly PathRegistryObjectType EVENTTYPE =
            new PathRegistryObjectType("event type", "An");

        public static readonly PathRegistryObjectType TABLE =
            new PathRegistryObjectType("table", "A");

        public static readonly PathRegistryObjectType VARIABLE =
            new PathRegistryObjectType("variable", "A");

        public static readonly PathRegistryObjectType EXPRDECL =
            new PathRegistryObjectType("declared-expression", "A");

        public static readonly PathRegistryObjectType SCRIPT =
            new PathRegistryObjectType("script", "A");

        public static readonly PathRegistryObjectType INDEX =
            new PathRegistryObjectType("index", "An");

        private PathRegistryObjectType(
            string name,
            string prefix)
        {
            Name = name;
            Prefix = prefix;
        }

        public string Name { get; }

        public string Prefix { get; }
    }
} // end of namespace