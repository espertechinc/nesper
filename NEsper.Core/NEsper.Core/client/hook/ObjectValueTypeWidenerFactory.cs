///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// For Avro use widener or transformer of object values to Avro record values
    /// </summary>
    public interface ObjectValueTypeWidenerFactory {
        /// <summary>
        /// Returns a type widener or coercer.
        /// <para>
        /// Implementations can provide custom widening behavior from an object to a a widened, coerced or related object value.
        /// </para>
        /// <para>
        /// Implementations should check whether an object value is assignable with or without coercion or widening.
        /// </para>
        /// <para>
        /// This method can return null to use the default widening behavior.
        /// </para>
        /// <para>
        /// Throw <seealso cref="UnsupportedOperationException" /> to indicate an unsupported widening or Coercion(default behavior checking still applies if no exception is thrown)
        /// </para>
        /// </summary>
        /// <param name="context">context</param>
        /// <exception cref="UnsupportedOperationException">to indicate an unsupported assignment (where not already covered by the default checking)</exception>
        /// <returns>coercer/widener</returns>
        TypeWidener Make(ObjectValueTypeWidenerFactoryContext context);
    }
} // end of namespace
