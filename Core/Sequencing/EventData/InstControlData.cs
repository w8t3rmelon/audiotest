using System;

namespace audiotest.Core.Sequencing.EventData;

public struct InstControlData
{
    public byte InstrumentRef;
    public string Param;
    
    public double? NewDoubleValue;
    public bool? NewBoolValue;
    public string? NewStringValue;
    public bool? TriggerAction;

    public void AssertValid()
    {
        byte valuesSet = 0;
        
        if (NewDoubleValue is not null) valuesSet++;
        if (NewBoolValue is not null) valuesSet++;
        if (NewStringValue is not null) valuesSet++;
        if (TriggerAction is not null) valuesSet++;
        
        if (valuesSet == 0)
            throw new InvalidOperationException("No value is set");
        if (valuesSet > 1)
            throw new InvalidOperationException("Multiple values are set");
    }
}