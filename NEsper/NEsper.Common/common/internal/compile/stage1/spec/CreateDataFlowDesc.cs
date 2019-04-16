///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class CreateDataFlowDesc
    {
        public CreateDataFlowDesc(
            String graphName,
            IList<GraphOperatorSpec> operators,
            IList<CreateSchemaDesc> schemas)
        {
            GraphName = graphName;
            Operators = operators;
            Schemas = schemas;
        }

        public string GraphName { get; private set; }

        public IList<GraphOperatorSpec> Operators { get; private set; }

        public IList<CreateSchemaDesc> Schemas { get; private set; }
    }
}