///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configures a variant stream.
    /// </summary>
    [Serializable]
    public class ConfigurationVariantStream
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ConfigurationVariantStream()
        {
            VariantTypeNames = new List<String>();
            TypeVariance = TypeVarianceEnum.PREDEFINED;
        }

        /// <summary>
        /// Returns the type variance setting specifying whether the variant stream accepts
        /// event of only the predefined types or any type.
        /// </summary>
        /// <returns>
        /// type variance setting
        /// </returns>
        public TypeVarianceEnum TypeVariance { get; set; }

        /// <summary>
        /// Returns the names of event types that a predefined for the variant stream.
        /// </summary>
        /// <returns>
        /// predefined types in the variant stream
        /// </returns>
        public IList<string> VariantTypeNames { get; private set; }

        /// <summary>
        /// Adds names of an event types that is one of the predefined event typs allowed
        /// for the variant stream.
        /// </summary>
        /// <param name="eventTypeName">name of the event type to allow in the variant stream</param>
        public void AddEventTypeName(String eventTypeName)
        {
            VariantTypeNames.Add(eventTypeName);
        }
    }

    /// <summary>
    /// Enumeration specifying whether only the predefine types or any type of event is
    /// accepted by the variant stream.
    /// </summary>
    public enum TypeVarianceEnum
    {
        /// <summary>
        /// Allow only the predefined types to be inserted into the stream.
        /// </summary>
        PREDEFINED,

        /// <summary>
        /// Allow any types to be inserted into the stream.
        /// </summary>
        ANY
    }
}
