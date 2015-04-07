///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Records minimal statement filter version required for processing.
    /// </summary>
    [Serializable]
    public class StatementAgentInstanceFilterVersion
    {
        /// <summary>Ctor. </summary>
        public StatementAgentInstanceFilterVersion()
        {
            StmtFilterVersion = long.MinValue;
        }

        /// <summary>Set filter version. </summary>
        /// <value>to set</value>
        public long StmtFilterVersion { get; set; }

        /// <summary>Check current filter. </summary>
        /// <param name="filterVersion">to check</param>
        /// <returns>false if not current</returns>
        public bool IsCurrentFilter(long filterVersion)
        {
            if (filterVersion < StmtFilterVersion)
            {
                // catch-up in case of roll
                if (filterVersion + 100000 < StmtFilterVersion && StmtFilterVersion != long.MaxValue)
                {
                    StmtFilterVersion = filterVersion;
                }
                return false;
            }
            return true;
        }
    }
}