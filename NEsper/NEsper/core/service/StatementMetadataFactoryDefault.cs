///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Statement metadata.
    /// </summary>
    public class StatementMetadataFactoryDefault : StatementMetadataFactory
    {
        public StatementMetadata Create(StatementMetadataFactoryContext context)
        {
            return new StatementMetadata(GetStatementType(context.StatementSpec, context.IsPattern));
        }
    
        public static StatementType GetStatementType(StatementSpecRaw statementSpec, bool pattern)
        {
            // determine statement type
            StatementType? statementType = null;
            if (statementSpec.CreateVariableDesc != null) {
                statementType = StatementType.CREATE_VARIABLE;
            }
            else if (statementSpec.CreateTableDesc != null) {
                statementType = StatementType.CREATE_TABLE;
            }
            else if (statementSpec.CreateWindowDesc != null) {
                statementType = StatementType.CREATE_WINDOW;
            }
            else if (statementSpec.OnTriggerDesc != null) {
                if (statementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) {
                    statementType = StatementType.ON_DELETE;
                }
                else if (statementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) {
                    statementType = StatementType.ON_UPDATE;
                }
                else if (statementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_SELECT) {
                    if (statementSpec.InsertIntoDesc != null) {
                        statementType = StatementType.ON_INSERT;
                    }
                    else {
                        statementType = StatementType.ON_SELECT;
                    }
                }
                else if (statementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_SET) {
                    statementType = StatementType.ON_SET;
                }
                else if (statementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE) {
                    statementType = StatementType.ON_MERGE;
                }
                else if (statementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_SPLITSTREAM) {
                    statementType = StatementType.ON_SPLITSTREAM;
                }
            }
            else if (statementSpec.InsertIntoDesc != null) {
                statementType = StatementType.INSERT_INTO;
            }
            else if (pattern) {
                statementType = StatementType.PATTERN;
            }
            else if (statementSpec.UpdateDesc != null) {
                statementType = StatementType.UPDATE;
            }
            else if (statementSpec.CreateIndexDesc != null) {
                statementType = StatementType.CREATE_INDEX;
            }
            else if (statementSpec.CreateContextDesc != null) {
                statementType = StatementType.CREATE_CONTEXT;
            }
            else if (statementSpec.CreateSchemaDesc != null) {
                statementType = StatementType.CREATE_SCHEMA;
            }
            else if (statementSpec.CreateDataFlowDesc != null) {
                statementType = StatementType.CREATE_DATAFLOW;
            }
            else if (statementSpec.CreateExpressionDesc != null) {
                statementType = StatementType.CREATE_EXPRESSION;
            }
            if (statementType == null) {
                statementType = StatementType.SELECT;
            }
            return statementType.Value;
        }
    }
}
