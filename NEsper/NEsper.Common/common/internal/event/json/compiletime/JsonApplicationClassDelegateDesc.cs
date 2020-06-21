///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class JsonApplicationClassDelegateDesc
    {
        public JsonApplicationClassDelegateDesc(
            string delegateClassName,
            string delegateFactoryClassName,
            IList<FieldInfo> fields)
        {
            DelegateClassName = delegateClassName;
            DelegateFactoryClassName = delegateFactoryClassName;
            Fields = fields;
        }

        public string DelegateClassName { get; }

        public string DelegateFactoryClassName { get; }

        public IList<FieldInfo> Fields { get; }
    }
} // end of namespace