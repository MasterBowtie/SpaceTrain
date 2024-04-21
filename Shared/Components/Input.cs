
namespace Shared.Components
{
    public class Input : Component
    {
        public enum Type : UInt16
        {
            Up,
            Down,
            Left,
            Right,
            Select,
            Exit,
            NE,
            NW,
            SE,
            SW,
            None
        }

        public Input(List<Type> inputs)
        {
            this.inputs = inputs;
        }

        public List<Type> inputs { get; private set; }
    }
}
