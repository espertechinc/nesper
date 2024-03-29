///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterServiceGranularLockFactoryNone : FilterServiceGranularLockFactory
    {
        public static readonly FilterServiceGranularLockFactoryNone Instance =
            new FilterServiceGranularLockFactoryNone();

        private static readonly IReaderWriterLock InstanceLock =
            new VoidReaderWriterLock();

        public IReaderWriterLock ObtainNew()
        {
            return InstanceLock;
        }
    }
}