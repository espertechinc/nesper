///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esperio.csv
{
    /// <summary>
    /// Coercer for using the constructor to perform the coercion.
    /// </summary>
    public class BasicTypeCoercer : AbstractTypeCoercer {
    
        public override object Coerce(string property, string source)
        {
            var factory = propertyFactories.Get(property);
            var value = factory != null ? factory.Invoke(source) : long.Parse(source);
    
            return value;
    	}
    }
}
