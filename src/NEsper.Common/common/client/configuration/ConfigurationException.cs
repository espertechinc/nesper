///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client.configuration
{
    /// <summary> Thrown to indicate a configuration problem.</summary>
    [Serializable]
    public sealed class ConfigurationException : EPException
    {
        /// <summary> Ctor.</summary>
        /// <param name="message">error message
        /// </param>
        public ConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary> Ctor for an inner exception and message.</summary>
        /// <param name="message">error message
        /// </param>
        /// <param name="cause">inner exception
        /// </param>
        public ConfigurationException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary> Ctor - just an inner exception.</summary>
        /// <param name="cause">inner exception
        /// </param>
        public ConfigurationException(Exception cause)
            : base(cause)
        {
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ConfigurationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}