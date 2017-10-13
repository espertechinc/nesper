///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>Object model of a data flow operator declaration. </summary>
    [Serializable]
    public class DataFlowOperator
    {
        /// <summary>Ctor </summary>
        /// <param name="annotations">annotations</param>
        /// <param name="operatorName">operator name</param>
        /// <param name="input">input stream definitions</param>
        /// <param name="output">output stream definitions</param>
        /// <param name="parameters">parameters</param>
        public DataFlowOperator(IList<AnnotationPart> annotations,
                                String operatorName,
                                IList<DataFlowOperatorInput> input,
                                IList<DataFlowOperatorOutput> output,
                                IList<DataFlowOperatorParameter> parameters)
        {
            Annotations = annotations;
            OperatorName = operatorName;
            Input = input;
            Output = output;
            Parameters = parameters;
        }

        /// <summary>Ctor. </summary>
        public DataFlowOperator()
        {
        }

        /// <summary>Returns the annotations. </summary>
        /// <value>annotations</value>
        public IList<AnnotationPart> Annotations { get; set; }

        /// <summary>Returns the operator name. </summary>
        /// <value>operator name</value>
        public string OperatorName { get; set; }

        /// <summary>Returns the input stream definitions, if any. </summary>
        /// <value>input streams</value>
        public IList<DataFlowOperatorInput> Input { get; set; }

        /// <summary>Returns the output stream definitions, if any. </summary>
        /// <value>output streams</value>
        public IList<DataFlowOperatorOutput> Output { get; set; }

        /// <summary>
        /// Returns operator parameters. <para> Object values may be expressions, constants, JSON values or EPL statements. </para>
        /// </summary>
        /// <value>map of parameters</value>
        public IList<DataFlowOperatorParameter> Parameters { get; set; }

        /// <summary>RenderAny to string. </summary>
        /// <param name="writer">to render</param>
        /// <param name="formatter">for formatting</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            writer.Write(OperatorName);

            if (Input.IsNotEmpty())
            {
                writer.Write("(");
                String delimiter = "";
                foreach (DataFlowOperatorInput inputItem in Input)
                {
                    writer.Write(delimiter);
                    WriteInput(inputItem, writer);
                    if (inputItem.OptionalAsName != null)
                    {
                        writer.Write(" as ");
                        writer.Write(inputItem.OptionalAsName);
                    }
                    delimiter = ", ";
                }
                writer.Write(")");
            }

            if (Output.IsNotEmpty())
            {
                writer.Write(" -> ");
                String delimiter = "";
                foreach (DataFlowOperatorOutput outputItem in Output)
                {
                    writer.Write(delimiter);
                    writer.Write(outputItem.StreamName);
                    WriteTypes(outputItem.TypeInfo, writer);
                    delimiter = ", ";
                }
            }

            if (Parameters.IsEmpty())
            {
                writer.Write(" {}");
                formatter.EndDataFlowOperatorDetails(writer);
            }
            else
            {
                writer.Write(" {");
                formatter.BeginDataFlowOperatorDetails(writer);
                String delimiter = ",";
                int count = 0;
                foreach (DataFlowOperatorParameter parameter in Parameters)
                {
                    parameter.ToEPL(writer);
                    count++;
                    if (Parameters.Count > count)
                    {
                        writer.Write(delimiter);
                        formatter.EndDataFlowOperatorConfig(writer);
                    }

                    formatter.EndDataFlowOperatorConfig(writer);
                }

                writer.Write("}");
                formatter.EndDataFlowOperatorDetails(writer);
            }
        }

        private void WriteInput(DataFlowOperatorInput inputItem, TextWriter writer)
        {
            if (inputItem.InputStreamNames.Count > 1)
            {
                String delimiterNames = "";
                writer.Write("(");
                foreach (String name in inputItem.InputStreamNames)
                {
                    writer.Write(delimiterNames);
                    writer.Write(name);
                    delimiterNames = ", ";
                }
                writer.Write(")");
            }
            else
            {
                writer.Write(inputItem.InputStreamNames[0]);
            }
        }

        private void WriteTypes(ICollection<DataFlowOperatorOutputType> types, TextWriter writer)
        {
            if (types == null || types.IsEmpty())
            {
                return;
            }

            writer.Write("<");
            String typeDelimiter = "";
            foreach (DataFlowOperatorOutputType type in types)
            {
                writer.Write(typeDelimiter);
                WriteType(type, writer);
                typeDelimiter = ",";
            }
            writer.Write(">");
        }

        private void WriteType(DataFlowOperatorOutputType type, TextWriter writer)
        {
            if (type.IsWildcard)
            {
                writer.Write('?');
                return;
            }
            writer.Write(type.TypeOrClassname);
            WriteTypes(type.TypeParameters, writer);
        }
    }
}