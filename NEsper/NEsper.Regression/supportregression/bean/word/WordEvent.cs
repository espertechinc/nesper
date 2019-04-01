///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean.word
{
    public class WordEvent {
        private readonly String _word;
    
        public WordEvent(String word) {
            _word = word;
        }

        public string Word
        {
            get { return _word; }
        }

        public String GetWord() {
            return _word;
        }
    }
}
