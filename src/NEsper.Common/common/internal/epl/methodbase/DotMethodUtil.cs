///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    public class DotMethodUtil
    {
        public static DotMethodFPProvided GetProvidedFootprint(IList<ExprNode> parameters)
        {
            var @params = new List<DotMethodFPProvidedParam>();
            foreach (var node in parameters) {
                if (!(node is ExprLambdaGoesNode goesNode)) {
                    @params.Add(new DotMethodFPProvidedParam(0, node.Forge.EvaluationType, node));
                    continue;
                }

                @params.Add(new DotMethodFPProvidedParam(goesNode.GoesToNames.Count, null, goesNode));
            }

            return new DotMethodFPProvided(@params.ToArray());
        }

        public static DotMethodFP ValidateParametersDetermineFootprint(
            DotMethodFP[] footprints,
            DotMethodTypeEnum methodType,
            string methodUsedName,
            DotMethodFPProvided providedFootprint,
            DotMethodInputTypeMatcher inputTypeMatcher)
        {
            var isLambdaApplies = DotMethodTypeEnum.ENUM == methodType;

            // determine footprint candidates strictly based on parameters
            List<DotMethodFP> candidates = null;
            DotMethodFP bestMatch = null;

            foreach (var footprint in footprints) {
                var requiredParams = footprint.Parameters;
                if (requiredParams.Length != providedFootprint.Parameters.Length) {
                    continue;
                }

                if (bestMatch == null) { // take first if number of parameters matches
                    bestMatch = footprint;
                }

                var paramMatch = true;
                var count = 0;
                foreach (var requiredParam in requiredParams) {
                    var providedParam = providedFootprint.Parameters[count++];
                    if (requiredParam.LambdaParamNum != providedParam.LambdaParamNum) {
                        paramMatch = false;
                    }
                }

                if (paramMatch) {
                    if (candidates == null) {
                        candidates = new List<DotMethodFP>();
                    }

                    candidates.Add(footprint);
                }
            }

            // if there are multiple candidates, eliminate by input (event bean collection or component collection)
            if (candidates != null && candidates.Count > 1) {
                candidates
                    .Where(fp => !inputTypeMatcher.Matches(fp))
                    .ToList()
                    .ForEach(fp => candidates.Remove(fp));
            }

            // handle single remaining candidate
            if (candidates != null && candidates.Count == 1) {
                var found = candidates[0];
                ValidateSpecificTypes(methodUsedName, methodType, found.Parameters, providedFootprint.Parameters);
                return found;
            }

            // check all candidates in detail to see which one matches, take first one
            if (candidates != null && !candidates.IsEmpty()) {
                bestMatch = candidates[0];
                var candidateIt = candidates.GetEnumerator();
                ExprValidationException firstException = null;
                while (candidateIt.MoveNext()) {
                    var fp = candidateIt.Current;
                    try {
                        ValidateSpecificTypes(methodUsedName, methodType, fp.Parameters, providedFootprint.Parameters);
                        return fp;
                    }
                    catch (ExprValidationException ex) {
                        if (firstException == null) {
                            firstException = ex;
                        }
                    }
                }

                if (firstException != null) {
                    throw firstException;
                }
            }

            var message = $"Parameters mismatch for {methodType.GetTypeName()} method '{methodUsedName}', the method ";
            if (bestMatch != null) {
                var buf = new StringWriter();
                buf.Write(bestMatch.ToStringFootprint(isLambdaApplies));
                buf.Write(", but receives ");
                buf.Write(DotMethodFP.ToStringProvided(providedFootprint, isLambdaApplies));
                throw new ExprValidationException(
                    $"{message}requires {buf}");
            }

            if (footprints.Length == 1) {
                throw new ExprValidationException(
                    $"{message}requires {footprints[0].ToStringFootprint(isLambdaApplies)}");
            }
            else {
                var buf = new StringWriter();
                var delimiter = "";
                foreach (var footprint in footprints) {
                    buf.Write(delimiter);
                    buf.Write(footprint.ToStringFootprint(isLambdaApplies));
                    delimiter = ", or ";
                }

                throw new ExprValidationException(
                    message +
                    "has multiple footprints accepting " +
                    buf +
                    ", but receives " +
                    DotMethodFP.ToStringProvided(providedFootprint, isLambdaApplies));
            }
        }

        private static void ValidateSpecificTypes(
            string methodUsedName,
            DotMethodTypeEnum type,
            DotMethodFPParam[] foundParams,
            DotMethodFPProvidedParam[] @params)
        {
            for (var i = 0; i < foundParams.Length; i++) {
                var found = foundParams[i];
                var provided = @params[i];

                // Lambda-type expressions not validated here
                if (found.LambdaParamNum > 0) {
                    continue;
                }

                EPLValidationUtil.ValidateParameterType(
                    methodUsedName,
                    type.GetTypeName(),
                    false,
                    found.ParamType,
                    found.SpecificType,
                    provided.ReturnType,
                    i,
                    provided.Expression);
            }
        }
    }
}