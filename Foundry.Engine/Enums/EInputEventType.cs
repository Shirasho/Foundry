namespace Foundry.Engine
{
    public enum EInputEventType : byte
    {
        Press,
        Drag,
        //TODO: Do we need to add an event for pressure-sensitive triggers?
        Release
    }
}
