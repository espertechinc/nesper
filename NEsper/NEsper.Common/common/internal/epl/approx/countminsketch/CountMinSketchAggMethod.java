
package com.espertech.esper.common.@internal.epl.approx.countminsketch;

import java.util.Locale;

public enum CountMinSketchAggMethod {
    FREQ("countMinSketchFrequency"),
    TOPK("countMinSketchTopk");

    private final String funcName;

    private CountMinSketchAggMethod(String funcName) {
        this.funcName = funcName;
    }

    public String getMethodName() {
        return funcName;
    }

    public static CountMinSketchAggMethod fromNameMayMatch(String name) {
        String nameLower = name.toLowerCase(Locale.ENGLISH);
        for (CountMinSketchAggMethod value : CountMinSketchAggMethod.values()) {
            if (value.funcName.toLowerCase(Locale.ENGLISH).equals(nameLower)) {
                return value;
            }
        }
        return null;
    }
}
