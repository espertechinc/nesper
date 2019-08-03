///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenChainPropertyElement : CodegenChainElement
    {
        private readonly string _propertyName;

        public CodegenChainPropertyElement(
            string propertyName)
        {
            this._propertyName = propertyName;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append(_propertyName);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
        }
    }
}