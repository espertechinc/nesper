///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esperio.csv;

namespace com.espertech.esperio.ext
{
    /// <summary>
    /// Date format coercion.
    /// </summary>
    public class BasicTypeCoercerDateFormat : BasicTypeCoercer
    {
    	public override object Coerce(string property, string source)
        {
    	    DateTime dateTime;
            if (DateTime.TryParse(source, out dateTime))
            {
                return dateTime;
            }
    	    return base.Coerce(property, source);
    	}
    }
}
