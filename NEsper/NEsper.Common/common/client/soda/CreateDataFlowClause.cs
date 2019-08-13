///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Represents a create-variable syntax for creating a new variable.
    /// </summary>
    [Serializable]
    public class CreateDataFlowClause
    {
        private string dataFlowName;
        private IList<DataFlowOperator> operators;
        private IList<CreateSchemaClause> schemas;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public CreateDataFlowClause()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="schemas">schemas</param>
        /// <param name="operators">operators</param>
        public CreateDataFlowClause(
            string dataFlowName,
            IList<CreateSchemaClause> schemas,
            IList<DataFlowOperator> operators)
        {
            this.dataFlowName = dataFlowName;
            this.schemas = schemas;
            this.operators = operators;
        }

        /// <summary>
        ///     Returns the data flow name.
        /// </summary>
        /// <returns>name</returns>
        public string DataFlowName
        {
            get => dataFlowName;
            set => dataFlowName = value;
        }

        /// <summary>
        ///     Returns schemas.
        /// </summary>
        /// <returns>schemas</returns>
        public IList<CreateSchemaClause> Schemas
        {
            get => schemas;
            set => schemas = value;
        }

        /// <summary>
        ///     Returns operators.
        /// </summary>
        /// <returns>operator definitions</returns>
        public IList<DataFlowOperator> Operators
        {
            get => operators;
            set => operators = value;
        }

        /// <summary>
        ///     Render as EPL.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">to use</param>
        public virtual void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("create dataflow ");
            writer.Write(dataFlowName);
            if (schemas != null)
            {
                foreach (var clause in schemas)
                {
                    formatter.BeginDataFlowSchema(writer);
                    clause.ToEPL(writer);
                    writer.Write(",");
                }
            }

            if (operators != null)
            {
                formatter.BeginDataFlowOperator(writer);
                foreach (var clause in operators)
                {
                    clause.ToEPL(writer, formatter);
                }
            }
        }
    }
} // end of namespace