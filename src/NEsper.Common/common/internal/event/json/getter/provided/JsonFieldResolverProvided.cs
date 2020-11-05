///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    public class JsonFieldResolverProvided
    {
	    /// <summary>
	    ///     NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="declaringClass">class</param>
	    /// <param name="fieldName">field name</param>
	    /// <returns>field</returns>
	    public static FieldInfo ResolveJsonField(
            Type declaringClass,
            string fieldName)
        {
            try {
                return declaringClass.GetField(fieldName);
            }
            catch (Exception ex) {
                throw new EPException("Failed to resolve field '" + fieldName + "' of declaring class '" + declaringClass.Name + "': " + ex.Message, ex);
            }
        }
    }
} // end of namespace