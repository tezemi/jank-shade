
namespace JankShade
{
    public struct Color
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        public static Color white => new Color(1f, 1f, 1f);

        public Color(float red, float green, float blue)
        {
            r = red;
            g = green;
            b = blue;
            a = 1f;
        }

        public Color(float red, float green, float blue, float alpha)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
        }

        public override string ToString()
        {
            return $"(RGBA: {r},{g},{b},{a})";
        }
    }
}
