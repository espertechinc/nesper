///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.db;

namespace com.espertech.esper.common.@internal.db
{
    public class StatelessDriverResolver : IDriverResolver
    {
        public DbDriver Resolve(Type driverType)
        {
            ArgumentNullException.ThrowIfNull(driverType);
            return (DbDriver)Activator.CreateInstance(driverType)!;
        }
    }
}