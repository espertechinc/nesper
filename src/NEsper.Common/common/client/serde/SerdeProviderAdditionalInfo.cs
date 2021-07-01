///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.client.serde
{
	/// <summary>
	///     For use with high-availability and scale-out only, this class provides additional
	///     information passed to serde provider, for use with
	///     <seealso cref="SerdeProvider" />
	/// </summary>
	public abstract class SerdeProviderAdditionalInfo
    {
        private readonly StatementRawInfo _raw;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="raw">statement information</param>
        public SerdeProviderAdditionalInfo(StatementRawInfo raw)
        {
            _raw = raw;
        }

        /// <summary>
        ///     Returns the statement name
        /// </summary>
        /// <value>name</value>
        public string StatementName => _raw.StatementName;

        /// <summary>
        ///     Returns the statement annotations
        /// </summary>
        /// <value>annotations</value>
        public Attribute[] Annotations => _raw.Annotations;

        /// <summary>
        ///     Returns the statement type
        /// </summary>
        /// <value>statement type</value>
        public StatementType StatementType => _raw.StatementType;

        /// <summary>
        ///     Returns the context name or null if no context associated
        /// </summary>
        /// <value>context name</value>
        public string ContextName => _raw.ContextName;

        /// <summary>
        ///     Returns the module name
        /// </summary>
        /// <value>module name</value>
        public string ModuleName => _raw.ModuleName;
    }
} // end of namespace