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
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;
using com.espertech.esper.epl.util;

namespace com.espertech.esper.epl.methodbase
{
    public class DotMethodUtil
    {
        public static DotMethodFPProvided GetProvidedFootprint(IList<ExprNode> parameters)
        {
            var @params = new List<DotMethodFPProvidedParam>();
            foreach (ExprNode node in parameters)
            {
                if (!(node is ExprLambdaGoesNode))
                {
                    @params.Add(new DotMethodFPProvidedParam(0, node.ExprEvaluator.ReturnType, node));
                    continue;
                }
                var goesNode = (ExprLambdaGoesNode)node;
                @params.Add(new DotMethodFPProvidedParam(goesNode.GoesToNames.Count, null, goesNode));
            }
            return new DotMethodFPProvided(@params.ToArray());
        }

        public static DotMethodFP ValidateParametersDetermineFootprint(DotMethodFP[] footprints, DotMethodTypeEnum methodType, String methodUsedName, DotMethodFPProvided providedFootprint, DotMethodInputTypeMatcher inputTypeMatcher)
        {
            Boolean isLambdaApplies = DotMethodTypeEnum.ENUM == methodType;

            // determine footprint candidates strictly based on parameters
            List<DotMethodFP> candidates = null;
            DotMethodFP bestMatch = null;

            foreach (var footprint in footprints)
            {
                var requiredParams = footprint.Parameters;
                if (requiredParams.Length != providedFootprint.Parameters.Length)
                {
                    continue;
                }

                if (bestMatch == null)
                {    // take first if number of parameters matches
                    bestMatch = footprint;
                }

                var paramMatch = true;
                var count = 0;
                foreach (var requiredParam in requiredParams)
                {
                    var providedParam = providedFootprint.Parameters[count++];
                    if (requiredParam.LambdaParamNum != providedParam.LambdaParamNum)
                    {
                        paramMatch = false;
                    }
                }

                if (paramMatch)
                {
                    if (candidates == null)
                    {
                        candidates = new List<DotMethodFP>();
                    }
                    candidates.Add(footprint);
                }
            }

            // if there are multiple candidates, eliminate by input (event bean collection or component collection)
            if (candidates != null && candidates.Count > 1)
            {
                candidates
                    .Where(fp => !inputTypeMatcher.Matches(fp))
                    .ToList()
                    .ForEach(fp => candidates.Remove(fp));
            }

            // handle single remaining candidate
            if (candidates != null && candidates.Count == 1)
            {
                DotMethodFP found = candidates[0];
                ValidateSpecificTypes(methodUsedName, methodType, found.Parameters, providedFootprint.Parameters);
                return found;
            }

            // check all candidates in detail to see which one matches, take first one
            if (candidates != null && !candidates.IsEmpty())
            {
                bestMatch = candidates[0];
                var candidateIt = candidates.GetEnumerator();
                ExprValidationException firstException = null;
                while (candidateIt.MoveNext() )
                {
                    DotMethodFP fp = candidateIt.Current;
                    try
                    {
                        ValidateSpecificTypes(methodUsedName, methodType, fp.Parameters, providedFootprint.Parameters);
                        return fp;
                    }
                    catch (ExprValidationException ex)
                    {
                        if (firstException == null)
                        {
                            firstException = ex;
                        }
                    }
                }
                if (firstException != null)
                {
                    throw firstException;
                }
            }
            var message = string.Format("Parameters mismatch for {0} method '{1}', the method ", methodType.GetTypeName(), methodUsedName);
            if (bestMatch != null)
            {
                var buf = new StringWriter();
                buf.Write(bestMatch.ToStringFootprint(isLambdaApplies));
                buf.Write(", but receives ");
                buf.Write(DotMethodFP.ToStringProvided(providedFootprint, isLambdaApplies));
                throw new ExprValidationException(
                    string.Format("{0}requires {1}", message, buf));
            }

            if (footprints.Length == 1)
            {
                throw new ExprValidationException(
                    string.Format("{0}requires {1}", message, footprints[0].ToStringFootprint(isLambdaApplies)));
            }
            else
            {
                var buf = new StringWriter();
                var delimiter = "";
                foreach (DotMethodFP footprint in footprints)
                {
                    buf.Write(delimiter);
                    buf.Write(footprint.ToStringFootprint(isLambdaApplies));
                    delimiter = ", or ";
                }

                throw new ExprValidationException(message + "has multiple footprints accepting " + buf +
                    ", but receives " + DotMethodFP.ToStringProvided(providedFootprint, isLambdaApplies));
            }
        }

        private static void ValidateSpecificTypes(String methodUsedName, DotMethodTypeEnum type, DotMethodFPParam[] foundParams, DotMethodFPProvidedParam[] @params)
        {
            for (int i = 0; i < foundParams.Length; i++)
            {
                DotMethodFPParam found = foundParams[i];
                DotMethodFPProvidedParam provided = @params[i];

                // Lambda-type expressions not validated here
                if (found.LambdaParamNum > 0)
                {
                    continue;
                }
                EPLValidationUtil.ValidateParameterType(
                    methodUsedName,
                    type.GetTypeName(), false, 
                    found.ParamType, 
                    found.SpecificType, 
                    provided.ReturnType, i, 
                    provided.Expression);
            }
        }
    }
}
