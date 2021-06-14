///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// For Avro use widener or transformer of object values to Avro record values
    /// </summary>
    public interface ObjectValueTypeWidenerFactory
    {
        /// <summary>
        /// Returns a type widener or coercer.
        /// <para />Implementations can provide custom widening behavior from an object to a a widened, coerced or related object value.
        /// <para />Implementations should check whether an object value is assignable with or without coercion or widening.
        /// <para />This method can return null to use the default widening behavior.
        /// <para />Throw <seealso cref="UnsupportedOperationException" /> to indicate an unsupported widening or coercion(default behavior checking still applies if no exception is thrown)
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>coercer/widener</returns>
        /// <throws>UnsupportedOperationException to indicate an unsupported assignment (where not already covered by the default checking)</throws>
        TypeWidenerSPI Make(ObjectValueTypeWidenerFactoryContext context);
    }
} // end of namespace