///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.client.serde
{
	/// <summary>
	///     For use with high-availability and scale-out only, this class provides information to the compiler how to
	///     resolve the serializer and de-serializer (serde) at deployment-time.
	/// </summary>
	public abstract class SerdeProvision
    {
	    /// <summary>
	    ///     Convert to serde forge
	    /// </summary>
	    /// <returns>serde forge</returns>
	    public abstract DataInputOutputSerdeForge ToForge();
    }
} // end of namespace