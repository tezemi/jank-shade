
namespace JankShade.Properties
{
    public struct Texture3D
    {
        public string Value { get; set; }

        public Texture3D(string value = "White")
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
