///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     Information about the multikey for which to obtain a serde.
    /// </summary>
    public class SerdeProviderAdditionalInfoMultikey : SerdeProviderAdditionalInfo
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="raw">statement information</param>
        public SerdeProviderAdditionalInfoMultikey(StatementRawInfo raw) : base(raw)
        {
        }

        public override string ToString()
        {
            return "multikey";
        }
    }
} // end of namespace