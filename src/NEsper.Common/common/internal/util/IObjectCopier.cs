///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    public interface IObjectCopier
    {
        /// <summary>
        /// Determines whether the copying the specified type is supported.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is supported; otherwise, <c>false</c>.
        /// </returns>
        bool IsSupported(Type type);

        /// <summary>
        /// Copies the input object.
        /// </summary>
        /// <param name="orig">is the object to be copied</param>
        /// <returns>copied object</returns>
        T Copy<T>(T orig);
    }
}