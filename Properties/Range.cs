
namespace JankShade.Properties
{
    public struct Range
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float Value { get; set; }

        public Range(float min, float max, float value)
        {
            Min = min;
            Max = max;
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
