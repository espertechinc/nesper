///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
    /// <summary>
    ///     Return result for unmap operators unmapping an intermal statement representation to the SODA object model.
    /// </summary>
    public class StatementSpecUnMapResult
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="objectModel">of the statement</param>
        public StatementSpecUnMapResult(EPStatementObjectModel objectModel)
        {
            ObjectModel = objectModel;
        }

        /// <summary>
        ///     Returns the object model.
        /// </summary>
        /// <returns>object model</returns>
        public EPStatementObjectModel ObjectModel { get; }
    }
} // end of namespace