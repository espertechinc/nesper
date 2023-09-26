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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Object model of a data flow operator declaration.
    /// </summary>
    [Serializable]
    public class DataFlowOperator
    {
        private IList<AnnotationPart> annotations;
        private string operatorName;
        private IList<DataFlowOperatorInput> input;
        private IList<DataFlowOperatorOutput> output;
        private IList<DataFlowOperatorParameter> parameters;

        /// <summary>Initializes a new instance of the <see cref="DataFlowOperator"/> class.</summary>
        /// <param name="annotations">The annotations.</param>
        /// <param name="operatorName">Name of the operator.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="parameters">The parameters.</param>
        public DataFlowOperator(
            IList<AnnotationPart> annotations,
            string operatorName,
            IList<DataFlowOperatorInput> input,
            IList<DataFlowOperatorOutput> output,
            IList<DataFlowOperatorParameter> parameters)
        {
            this.annotations = annotations;
            this.operatorName = operatorName;
            this.input = input;
            this.output = output;
            this.parameters = parameters;
        }

        /// <summary>Initializes a new instance of the <see cref="DataFlowOperator"/> class.</summary>
        public DataFlowOperator()
        {
        }

        public IList<AnnotationPart> Annotations {
            get => annotations;
            set => annotations = value;
        }

        public string OperatorName {
            get => operatorName;
            set => operatorName = value;
        }

        public IList<DataFlowOperatorInput> Input {
            get => input;
            set => input = value;
        }

        public IList<DataFlowOperatorOutput> Output {
            get => output;
            set => output = value;
        }

        public IList<DataFlowOperatorParameter> Parameters {
            get => parameters;
            set => parameters = value;
        }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write(operatorName);

            if (!input.IsEmpty()) {
                writer.Write("(");
                var delimiter = "";
                foreach (var inputItem in input) {
                    writer.Write(delimiter);
                    WriteInput(inputItem, writer);
                    if (inputItem.OptionalAsName != null) {
                        writer.Write(" as ");
                        writer.Write(inputItem.OptionalAsName);
                    }

                    delimiter = ", ";
                }

                writer.Write(")");
            }

            if (!output.IsEmpty()) {
                writer.Write(" -> ");
                var delimiter = "";
                foreach (var outputItem in output) {
                    writer.Write(delimiter);
                    writer.Write(outputItem.StreamName);
                    WriteTypes(outputItem.TypeInfo, writer);
                    delimiter = ", ";
                }
            }

            if (parameters.IsEmpty()) {
                writer.Write(" {}");
                formatter.EndDataFlowOperatorDetails(writer);
            }
            else {
                writer.Write(" {");
                formatter.BeginDataFlowOperatorDetails(writer);
                var delimiter = ",";
                var count = 0;
                foreach (var parameter in parameters) {
                    parameter.ToEPL(writer);
                    count++;
                    if (parameters.Count > count) {
                        writer.Write(delimiter);
                    }

                    formatter.EndDataFlowOperatorConfig(writer);
                }

                writer.Write("}");
                formatter.EndDataFlowOperatorDetails(writer);
            }
        }

        private void WriteInput(
            DataFlowOperatorInput inputItem,
            TextWriter writer)
        {
            if (inputItem.InputStreamNames.Count > 1) {
                var delimiterNames = "";
                writer.Write("(");
                foreach (var name in inputItem.InputStreamNames) {
                    writer.Write(delimiterNames);
                    writer.Write(name);
                    delimiterNames = ", ";
                }

                writer.Write(")");
            }
            else {
                writer.Write(inputItem.InputStreamNames[0]);
            }
        }

        private void WriteTypes(
            ICollection<DataFlowOperatorOutputType> types,
            TextWriter writer)
        {
            if (types == null || types.IsEmpty()) {
                return;
            }

            writer.Write("<");
            var typeDelimiter = "";
            foreach (var type in types) {
                writer.Write(typeDelimiter);
                WriteType(type, writer);
                typeDelimiter = ",";
            }

            writer.Write(">");
        }

        private void WriteType(
            DataFlowOperatorOutputType type,
            TextWriter writer)
        {
            if (type.IsWildcard) {
                writer.Write('?');
                return;
            }

            writer.Write(type.TypeOrClassname);
            WriteTypes(type.TypeParameters, writer);
        }
    }
}