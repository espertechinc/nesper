///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.dataflow.util
{
    public class OperatorMetadataDescriptor
    {
        public OperatorMetadataDescriptor(GraphOperatorSpec operatorSpec,
                                          int operatorNumber,
                                          Type operatorClass,
                                          Type operatorFactoryClass,
                                          Object optionalOperatorObject,
                                          String operatorPrettyPrint,
                                          Attribute[] operatorAnnotations)
        {
            OperatorSpec = operatorSpec;
            OperatorNumber = operatorNumber;
            OperatorClass = operatorClass;
            OperatorFactoryClass = operatorFactoryClass;
            OptionalOperatorObject = optionalOperatorObject;
            OperatorPrettyPrint = operatorPrettyPrint;
            OperatorAnnotations = operatorAnnotations;
        }

        public GraphOperatorSpec OperatorSpec { get; private set; }

        public string OperatorName
        {
            get { return OperatorSpec.OperatorName; }
        }

        public Type OperatorClass { get; private set; }

        public Type OperatorFactoryClass { get; private set; }

        public object OptionalOperatorObject { get; private set; }

        public int OperatorNumber { get; private set; }

        public string OperatorPrettyPrint { get; private set; }

        public Attribute[] OperatorAnnotations { get; private set; }
    }
}