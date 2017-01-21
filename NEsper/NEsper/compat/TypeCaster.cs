///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Casts an opaque object into a different type of opaque object.
    /// The TypeCaster encapsulates the behavior to make the transformation
    /// occur but the input and output types are loosely defined.
    /// </summary>
    /// <param name="sourceObj"></param>
    /// <returns></returns>

    public delegate Object TypeCaster(Object sourceObj);

    /// <summary>
    /// Casts (not converts) an opaque object into a strongly typed result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceObj"></param>
    /// <returns></returns>

    public delegate T GenericTypeCaster<T>(Object sourceObj);
}
