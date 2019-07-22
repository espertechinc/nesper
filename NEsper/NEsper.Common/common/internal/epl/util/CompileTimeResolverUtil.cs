///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.epl.util
{
    public static class CompileTimeResolverUtil
    {
        public static T ValidateAmbiguous<T>(
            T local,
            T path,
            T preconfigured,
            PathRegistryObjectType objectType,
            String name)
        {
            if (path != null && preconfigured != null) {
                throw new EPException(
                    "The " +
                    objectType.Name +
                    " by name '" +
                    name +
                    "' is ambiguous as it exists in both the path space and the preconfigured space");
            }

            if (local != null) {
                if (path != null || preconfigured != null) {
                    // This should not happen as any create-XXX has checked whether if it already exists; handle it anyway
                    throw new EPException(
                        "The " +
                        objectType.Name +
                        " by name '" +
                        name +
                        "' is ambiguous as it exists in both the local space and the path or preconfigured space");
                }

                return local;
            }

            return path != null ? path : preconfigured;
        }

        public static EPException MakePathAmbiguous(
            PathRegistryObjectType objectType,
            String name,
            PathException e)
        {
            return new EPException(
                "The " +
                objectType.Name +
                " by name '" +
                name +
                "' is ambiguous as it exists for multiple modules: " +
                e.Message,
                e);
        }
    }
}