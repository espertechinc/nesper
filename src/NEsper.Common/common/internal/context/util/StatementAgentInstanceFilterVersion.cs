///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Records minimal statement filter version required for processing.
    /// </summary>
    [Serializable]
    public class StatementAgentInstanceFilterVersion
    {
        private long _stmtFilterVersion;

        /// <summary>Ctor. </summary>
        public StatementAgentInstanceFilterVersion()
        {
            _stmtFilterVersion = long.MinValue;
        }

        /// <summary>Set filter version. </summary>
        /// <value>to set</value>
        public long StmtFilterVersion {
            get => _stmtFilterVersion;
            set => _stmtFilterVersion = value;
        }

        /// <summary>Check current filter. </summary>
        /// <param name="filterVersion">to check</param>
        /// <returns>false if not current</returns>
        public bool IsCurrentFilter(long filterVersion)
        {
            if (filterVersion < _stmtFilterVersion) {
                // catch-up in case of roll
                if (filterVersion + 100000 < _stmtFilterVersion && _stmtFilterVersion != long.MaxValue) {
                    _stmtFilterVersion = filterVersion;
                }

                return false;
            }

            return true;
        }
    }
}