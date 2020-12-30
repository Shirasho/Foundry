using System;
using System.Threading.Tasks;

namespace Foundry.Engine
{
    public abstract class Actor : GameObject
    {
        protected void BindInput(string bindingName, Action<InputEventArgs> action)
        {

        }

        protected void BindInput(string bindingName, Func<InputEventArgs, ValueTask> action)
        {

        }
    }
}
