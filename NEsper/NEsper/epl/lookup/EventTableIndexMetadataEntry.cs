///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class EventTableIndexMetadataEntry : EventTableIndexEntryBase
    {
        private readonly bool _primary;
        private readonly ISet<string> _referencedByStmt;
        private readonly QueryPlanIndexItem _queryPlanIndexItem;
        private readonly string _explicitIndexNameIfExplicit;

        public EventTableIndexMetadataEntry(string optionalIndexName, bool primary, QueryPlanIndexItem queryPlanIndexItem, string explicitIndexNameIfExplicit)
            : base(optionalIndexName)
        {
            _primary = primary;
            _queryPlanIndexItem = queryPlanIndexItem;
            _referencedByStmt = primary ? null : new HashSet<string>();
            _explicitIndexNameIfExplicit = explicitIndexNameIfExplicit;
        }
    
        public void AddReferringStatement(string statementName)
        {
            if (!_primary) {
                _referencedByStmt.Add(statementName);
            }
        }
    
        public bool RemoveReferringStatement(string referringStatementName)
        {
            if (!_primary) {
                _referencedByStmt.Remove(referringStatementName);
                if (_referencedByStmt.IsEmpty()) {
                    return true;
                }
            }
            return false;
        }

        public bool IsPrimary => _primary;

        public string[] ReferringStatements => _referencedByStmt.ToArray();

        public QueryPlanIndexItem QueryPlanIndexItem => _queryPlanIndexItem;

        public string ExplicitIndexNameIfExplicit => _explicitIndexNameIfExplicit;
    }
}
