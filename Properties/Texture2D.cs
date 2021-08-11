
namespace JankShade.Properties
{
    public struct Texture2D
    {
        public string Value { get; set; }

        public Texture2D(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"\"{Value}\" " + "{}";
        }
    }
}
