///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.json.getter.core
{
    public interface JsonEventPropertyGetter : EventPropertyGetterSPI
    {
        object GetJsonProp(object @object);

        bool GetJsonExists(object @object);

        object GetJsonFragment(object @object);
    }
} // end of namespace