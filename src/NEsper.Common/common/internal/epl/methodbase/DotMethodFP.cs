///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    [Serializable]
    public class DotMethodFP
    {
        public DotMethodFP(
            DotMethodFPInputEnum input,
            params DotMethodFPParam[] parameters)
        {
            Input = input;
            Parameters = parameters;
        }

        public DotMethodFPInputEnum Input { get; private set; }

        public DotMethodFPParam[] Parameters { get; private set; }

        public string ToStringFootprint(bool isLambdaApplies)
        {
            if (Parameters.Length == 0) {
                return "no parameters";
            }

            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var param in Parameters) {
                buf.Append(delimiter);

                if (isLambdaApplies) {
                    if (param.LambdaParamNum == 0) {
                        buf.Append("an (non-lambda)");
                    }
                    else if (param.LambdaParamNum == 1) {
                        buf.Append("a lambda");
                    }
                    else {
                        buf.Append("a " + param.LambdaParamNum + "-parameter lambda");
                    }
                }
                else {
                    buf.Append("an");
                }

                buf.Append(" expression");
                buf.Append(" providing ");
                buf.Append(param.Description);
                delimiter = " and ";
            }

            return buf.ToString();
        }

        public static string ToStringProvided(
            DotMethodFPProvided provided,
            bool isLambdaApplies)
        {
            if (provided.Parameters.Length == 0) {
                return "no parameters";
            }

            var buf = new StringWriter();
            var delimiter = "";

            if (!isLambdaApplies) {
                buf.Write(provided.Parameters.Length);
                buf.Write(" expressions");
            }
            else {
                foreach (var param in provided.Parameters) {
                    buf.Write(delimiter);

                    if (param.LambdaParamNum == 0) {
                        buf.Write("an (non-lambda)");
                    }
                    else if (param.LambdaParamNum == 1) {
                        buf.Write("a lambda");
                    }
                    else {
                        buf.Write("a " + param.LambdaParamNum + "-parameter lambda");
                    }

                    buf.Write(" expression");
                    delimiter = " and ";
                }
            }

            return buf.ToString();
        }
    }
}