using System;
using JankShade.Properties;
using Range = JankShade.Properties.Range;

namespace JankShade
{
    public struct ShaderProperty<T>
    {
        public bool GenerateVariable { get; set; }
        public string VariableName { get; set; }
        public string NiceName { get; set; }
        public T DefaultValue { get; set; }
        public Precision? Precision { get; set; }
        private const float FIXED_FLOAT_RANGE = 2f;
        private const float HALF_FLOAT_RANGE = 60000;

        public ShaderProperty(string variableName, string niceName, T defaultValue, bool generateVariable = true)
        {
            VariableName = variableName;
            NiceName = niceName;
            DefaultValue = defaultValue;
            GenerateVariable = generateVariable;
            Precision = null;
        }

        public ShaderProperty(string variableName, string niceName, T defaultValue, Precision precision, bool generateVariable = true)
        {
            VariableName = variableName;
            NiceName = niceName;
            DefaultValue = defaultValue;
            GenerateVariable = generateVariable;
            Precision = precision;
        }

        public override string ToString()
        {
            return $"{VariableName}(\"{NiceName}\", {GetPropertyName(DefaultValue)}) = {DefaultValue}";
        }

        public static string GetPropertyName(object property)
        {
            switch (property)
            {
                case int:
                    return "Int";
                case float:
                    return "Float";
                case Range r:
                    return $"Range({r.Min}, {r.Max})";
                case Texture2D:
                    return "2D";
                case Texture2DArray:
                    return "2DArray";
                case Texture3D:
                    return "3D";
                case Cubemap:
                    return "Cube";
                case CubemapArray:
                    return "CubeArray";
                case Color:
                    return "Color";
                case Vector:
                    return "Vector";
                default:
                    throw new ArgumentException("Couldn't get property name. The property is not a valid property type.");
            }
        }

        public static string GetPropertyDataType(object property)
        {
            switch (property)
            {
                case ShaderProperty<int> i:
                    return $"int {i.VariableName}";
                case ShaderProperty<float> fl:
                    if (fl.Precision.HasValue)
                    {
                        switch (fl.Precision.Value)
                        {
                            case Properties.Precision.Float:
                                return $"float {fl.VariableName}";
                            case Properties.Precision.Half:
                                return $"half {fl.VariableName}";
                            case Properties.Precision.Fixed:
                                return $"fixed {fl.VariableName}";
                        }
                    }

                    if (fl.DefaultValue > -FIXED_FLOAT_RANGE && fl.DefaultValue < FIXED_FLOAT_RANGE)
                    {
                        return $"fixed {fl.VariableName}";
                    }
                    
                    if (fl.DefaultValue > -HALF_FLOAT_RANGE && fl.DefaultValue < HALF_FLOAT_RANGE)
                    {
                        return $"half {fl.VariableName}";
                    }

                    return $"float {fl.VariableName}";
                case ShaderProperty<Range> range:
                    if (range.Precision.HasValue)
                    {
                        switch (range.Precision.Value)
                        {
                            case Properties.Precision.Float:
                                return $"float {range.VariableName}";
                            case Properties.Precision.Half:
                                return $"half {range.VariableName}";
                            case Properties.Precision.Fixed:
                                return $"fixed {range.VariableName}";
                        }
                    }

                    if (range.DefaultValue.Min > -FIXED_FLOAT_RANGE && range.DefaultValue.Max < FIXED_FLOAT_RANGE)
                    {
                        return $"fixed {range.VariableName}";
                    }

                    if (range.DefaultValue.Min > -HALF_FLOAT_RANGE && range.DefaultValue.Max < HALF_FLOAT_RANGE)
                    {
                        return $"half {range.VariableName}";
                    }

                    return $"float {range.VariableName}";
                case ShaderProperty<Texture2D> texture2d:
                    if (texture2d.Precision.HasValue)
                    {
                        switch (texture2d.Precision.Value)
                        {
                            case Properties.Precision.Float:
                                return $"sampler2D_float {texture2d.VariableName}";
                            case Properties.Precision.Half:
                                return $"sampler2D_half {texture2d.VariableName}";
                            case Properties.Precision.Fixed:
                                return $"sampler2D {texture2d.VariableName}";
                        }
                    }

                    return $"sampler2D {texture2d.VariableName}";
                case ShaderProperty<Texture2DArray> _:
                case ShaderProperty<Texture3D> _:
                case ShaderProperty<Cubemap> _:             // TODO
                case ShaderProperty<CubemapArray> _:
                case ShaderProperty<Color> _:
                case ShaderProperty<Vector> _:
                    break;
            }

            throw new ArgumentException();
        }
    }
}
