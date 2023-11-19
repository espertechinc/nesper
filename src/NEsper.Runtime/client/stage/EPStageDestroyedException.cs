///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client.stage
{
	/// <summary>
	///     This exception is thrown to indicate that the runtime instance has been destroyed.
	///     <para />
	///     This exception applies to destroyed runtime when a client attempts to use the runtime after it was destroyed.
	/// </summary>
	public class EPStageDestroyedException : EPRuntimeException
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="runtimeURI">runtime URI</param>
	    public EPStageDestroyedException(string runtimeURI) : base("Runtime has already been destroyed for runtime URI '" + runtimeURI + "'")
        {
        }

        protected EPStageDestroyedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace