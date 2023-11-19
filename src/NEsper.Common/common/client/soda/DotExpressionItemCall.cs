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
    /// Dot-expression item representing a call that has an name and parameters.
    /// </summary>
    public class DotExpressionItemCall : DotExpressionItem
    {
        private string _name;
        private IList<Expression> _parameters;

        public string Name {
            get => _name;
            set => _name = value;
        }

        public IList<Expression> Parameters {
            get => _parameters;
            set => _parameters = value;
        }

        public DotExpressionItemCall()
        {
        }

        public DotExpressionItemCall(
            string name,
            IList<Expression> parameters)
        {
            _name = name;
            _parameters = parameters;
        }

        public override void RenderItem(TextWriter writer)
        {
            writer.Write(_name);
            writer.Write("(");
            var delimiter = "";
            foreach (var param in _parameters) {
                writer.Write(delimiter);
                delimiter = ",";
                param.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(")");
        }
    }
}