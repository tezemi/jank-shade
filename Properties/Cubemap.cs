
namespace JankShade.Properties
{
    public struct Cubemap
    {
        public string Value { get; set; }

        public Cubemap(string value = "White")
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
