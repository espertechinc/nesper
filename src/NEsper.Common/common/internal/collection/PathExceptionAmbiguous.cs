///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.collection
{
    public class PathExceptionAmbiguous : PathException
    {
        public PathExceptionAmbiguous(
            string name,
            PathRegistryObjectType objectType)
            : base(
                objectType.Prefix + " " + objectType.Name + " by name '" + name + "' is exported by multiple modules")
        {
        }
    }
} // end of namespace