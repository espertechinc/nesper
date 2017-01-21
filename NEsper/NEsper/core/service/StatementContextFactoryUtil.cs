///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.core.service
{
    public class StatementContextFactoryUtil
    {
        public static bool DetermineHasTableAccess(IList<ExprSubselectNode> subselectNodes, StatementSpecRaw statementSpecRaw, EPServicesContext engineServices) {
            var hasTableAccess = (statementSpecRaw.TableExpressions != null && !statementSpecRaw.TableExpressions.IsEmpty()) ||
                    statementSpecRaw.IntoTableSpec != null;
            hasTableAccess = hasTableAccess || IsJoinWithTable(statementSpecRaw, engineServices.TableService) || IsSubqueryWithTable(subselectNodes, engineServices.TableService) || IsInsertIntoTable(statementSpecRaw, engineServices.TableService);
            return hasTableAccess;
        }
    
        private static bool IsInsertIntoTable(StatementSpecRaw statementSpecRaw, TableService tableService) {
            if (statementSpecRaw.InsertIntoDesc == null) {
                return false;
            }
            return tableService.GetTableMetadata(statementSpecRaw.InsertIntoDesc.EventTypeName) != null;
        }
    
        private static bool IsSubqueryWithTable(IList<ExprSubselectNode> subselectNodes, TableService tableService) {
            foreach (var node in subselectNodes) {
                var spec = (FilterStreamSpecRaw) node.StatementSpecRaw.StreamSpecs[0];
                if (tableService.GetTableMetadata(spec.RawFilterSpec.EventTypeName) != null) {
                    return true;
                }
            }
            return false;
        }
    
        private static bool IsJoinWithTable(StatementSpecRaw statementSpecRaw, TableService tableService) {
            foreach (var stream in statementSpecRaw.StreamSpecs) {
                if (stream is FilterStreamSpecRaw) {
                    var filter = (FilterStreamSpecRaw) stream;
                    if (tableService.GetTableMetadata(filter.RawFilterSpec.EventTypeName) != null) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
