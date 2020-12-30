using System;

namespace Foundry.Engine
{
    public class InputEventArgs : EventArgs
    {
        public EInputEventType EventType { get; }

        public double OldValue { get; }

        public double NewValue { get; }

        public double Delta => NewValue - OldValue;

        public InputEventArgs(EInputEventType eventType)
        {
            EventType = eventType;
        }

        public InputEventArgs(EInputEventType eventType, double oldValue, double newValue)
        {
            EventType = eventType;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
