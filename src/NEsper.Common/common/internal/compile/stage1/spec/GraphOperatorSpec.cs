///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class GraphOperatorSpec
    {
        public GraphOperatorSpec(
            string operatorName,
            GraphOperatorInput input,
            GraphOperatorOutput output,
            GraphOperatorDetail detail,
            IList<AnnotationDesc> annotations)
        {
            OperatorName = operatorName;
            Input = input;
            Output = output;
            Detail = detail;
            Annotations = annotations;
        }

        public string OperatorName { get; private set; }

        public GraphOperatorInput Input { get; private set; }

        public GraphOperatorOutput Output { get; private set; }

        public GraphOperatorDetail Detail { get; private set; }

        public IList<AnnotationDesc> Annotations { get; private set; }
    }
}