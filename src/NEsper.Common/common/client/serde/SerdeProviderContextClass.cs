///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     For use with high-availability and scale-out only, this class provides contextual information about the class that we
    ///     looking to serialize or de-serialize, for use with <seealso cref="SerdeProvider" />
    /// </summary>
    public class SerdeProviderContextClass
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="clazz">type</param>
        /// <param name="additionalInfo">additional information on why and where a serde is need for this type</param>
        public SerdeProviderContextClass(
            Type clazz,
            SerdeProviderAdditionalInfo additionalInfo)
        {
            Clazz = clazz;
            AdditionalInfo = additionalInfo;
        }

        /// <summary>
        ///     Returns the type to provide a serde for
        /// </summary>
        /// <value>type</value>
        public Type Clazz { get; }

        /// <summary>
        ///     Returns additional information on why and where a serde is need for this type
        /// </summary>
        /// <value>info</value>
        public SerdeProviderAdditionalInfo AdditionalInfo { get; }
    }
} // end of namespace