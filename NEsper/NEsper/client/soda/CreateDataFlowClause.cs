///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>Represents a create-variable syntax for creating a new variable. </summary>
    [Serializable]
    public class CreateDataFlowClause
    {
        /// <summary>Ctor. </summary>
        public CreateDataFlowClause() {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="schemas">schemas</param>
        /// <param name="operators">operators</param>
        public CreateDataFlowClause(String dataFlowName, IList<CreateSchemaClause> schemas, IList<DataFlowOperator> operators)
        {
            DataFlowName = dataFlowName;
            Schemas = schemas;
            Operators = operators;
        }

        /// <summary>Returns the data flow name. </summary>
        /// <value>name</value>
        public string DataFlowName { get; set; }

        /// <summary>Returns schemas. </summary>
        /// <value>schemas</value>
        public IList<CreateSchemaClause> Schemas { get; set; }

        /// <summary>Returns operators. </summary>
        /// <value>operator definitions</value>
        public IList<DataFlowOperator> Operators { get; set; }

        /// <summary>Render as EPL. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">to use</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            writer.Write("create dataflow ");
            writer.Write(DataFlowName);
            if (Schemas != null) {
                foreach (CreateSchemaClause clause in Schemas) {
                    formatter.BeginDataFlowSchema(writer);
                    clause.ToEPL(writer);
                    writer.Write(",");
                }
            }
            if (Operators != null) {
                formatter.BeginDataFlowOperator(writer);
                foreach (DataFlowOperator clause in Operators) {
                    clause.ToEPL(writer, formatter);
                }
            }
        }
    }
}
