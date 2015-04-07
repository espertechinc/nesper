///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.rettype
{
    /// <summary>
    /// Any primitive type as well as any class and other non-array or non-collection type
    /// </summary>
    public class ClassEPType : EPType
    {
        private readonly Type _type;
    
        internal ClassEPType(Type type) {
            _type = type;
        }

        public Type Clazz
        {
            get { return _type; }
        }
    }
}
