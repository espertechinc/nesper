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
    public class SentenceEvent {
        private readonly String _sentence;
    
        public SentenceEvent(String sentence) {
            _sentence = sentence;
        }
    
        public WordEvent[] GetWords() {
            String[] split = _sentence.Split(' ');
            WordEvent[] words = new WordEvent[split.Length];
            for (int i = 0; i < split.Length; i++) {
                words[i] = new WordEvent(split[i]);
            }
            return words;
        }
    }
    
}
