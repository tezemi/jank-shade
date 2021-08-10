
using JankShade.Properties;

namespace JankShade
{
    public struct ShaderProperty<T>
    {
        public string PropertyName { get; set; }
        public string EditorName { get; set; }
        public T DefaultValue { get; set; }
        public bool GenerateDefinition { get; set; }

        public ShaderProperty(string propertyName, string editorName, T defaultValue, bool generateDefinition = true)
        {
            PropertyName = propertyName;
            EditorName = editorName;
            DefaultValue = defaultValue;
            GenerateDefinition = generateDefinition;
        }

        public override string ToString()
        {
            var type = string.Empty;
            object value = null;
            switch (DefaultValue)
            {
                case int i:
                    type = "Int";
                    value = i;
                    break;
                case float f:
                    type = "Float";
                    value = f;
                    break;
                case Range r:
                    type = $"Range({r.Min}, {r.Max})";
                    value = r.Value;
                    break;
                case Texture2D t:
                    type = "2D";
                    value = "\"White\" {}";
                    break;
                case Texture2DArray:
                    type = "2DArray";
                    break;
                case Texture3D:
                    type = "3D";
                    break;
                case Cubemap:
                    type = "Cube";
                    break;
                case CubemapArray:
                    type = "CubeArray";
                    break;
                case Color c:
                    type = "Color";
                    value = c;
                    break;
                case Vector v:
                    type = "Vector";
                    value = v;
                    break;
            }

            return $"{PropertyName}(\"{EditorName}\", {type}) = {value}";
        }
    }
}
