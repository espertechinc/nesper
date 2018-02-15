///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.supportregression.bean
{
    public class ISupportDImpl : ISupportD
    {
        private readonly String valueBaseD;
        private readonly String valueBaseDBase;
        private readonly String valueD;

        public ISupportDImpl(String valueD, String valueBaseD, String valueBaseDBase)
        {
            this.valueD = valueD;
            this.valueBaseD = valueBaseD;
            this.valueBaseDBase = valueBaseDBase;
        }

        #region ISupportD Members

        public virtual String D
        {
            get { return valueD; }
        }

        public virtual String BaseD
        {
            get { return valueBaseD; }
        }

        public virtual String BaseDBase
        {
            get { return valueBaseDBase; }
        }

        #endregion
    }
}
