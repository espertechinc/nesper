///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.collection
{
    public class PathExceptionAlreadyRegistered : PathException
    {
        public PathExceptionAlreadyRegistered(
            string name,
            PathRegistryObjectType objectType,
            string moduleName)
            : base(
                objectType.Prefix +
                " " +
                objectType.Name +
                " by name '" +
                name +
                "' has already been created for module '" +
                StringValue.UnnamedWhenNullOrEmpty(moduleName) +
                "'")
        {
        }
    }
} // end of namespace