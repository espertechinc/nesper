///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.rettype
{
    /// <summary>
    /// An array or collection of native values. Always has a component type. 
    /// Either: - array then "clazz.Array" returns true. - collection then clazz : collection
    /// </summary>
    public class ClassMultiValuedEPType : EPType
    {
        private readonly Type _container;
        private readonly Type _component;

        internal ClassMultiValuedEPType(Type container, Type component)
        {
            _container = container;
            _component = component;
        }

        public Type Container
        {
            get { return _container; }
        }

        public Type Component
        {
            get { return _component; }
        }
    }
}
