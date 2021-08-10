using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using JankShade.Properties;
using Range = JankShade.Properties.Range;

namespace JankShade
{
    public class Shader
    {
        public string Name { get; set; }
        public Program Program { get; set; }
        private string _fallback;
        private List<object> _shaderProperties = new List<object>();
        private List<string> _commands = new List<string>();
        private Dictionary<string, string> _tags = new Dictionary<string, string>();
        private List<string> _pragmaticDirectives = new List<string>();
        private OrderedDictionary _functions = new OrderedDictionary();
        private Dictionary<string, string> _structs = new Dictionary<string, string>();
        private const string STRUCT_REGEX = "^struct [A-z]*";
        private const string FUNCTION_SIGNATURE_REGEX = @"^[a-z]*[0-9]*(?:\s+\w+)*\s*\(([^),]*)(\s*,\s*[^),]*){0,}\)";

        public Shader(string name, Program program)
        {
            Name = name;
        }

        public Shader AddProperties(params object[] properties)
        {
            foreach (var obj in properties)
            {
                switch (obj)
                {
                    case ShaderProperty<int> _:
                    case ShaderProperty<float> _:
                    case ShaderProperty<Range> _:
                    case ShaderProperty<Texture2D> _:
                    case ShaderProperty<Texture2DArray> _:
                    case ShaderProperty<Texture3D> _:
                    case ShaderProperty<Cubemap> _:
                    case ShaderProperty<CubemapArray> _:
                    case ShaderProperty<Color> _:
                    case ShaderProperty<Vector> _:
                        _shaderProperties.Add(obj);
                        break;
                    default:
                        throw new ArgumentException("Property must be a ShaderProperty<T>, with an applicable type.");
                }
            }

            return this;
        }

        public Shader AddCommands(params string[] commands)
        {
            foreach (var s in commands)
            {
                _commands.Add(s);
            }

            return this;
        }

        public Shader AddTags(params (string, string)[] tags)
        {
            foreach (var (k, v) in tags)
            {
                _tags.Add(k, v);
            }

            return this;
        }

        public Shader SetTags(params (string, string)[] tags)
        {
            _tags.Clear();

            foreach (var (k, v) in tags)
            {
                _tags.Add(k, v);
            }

            return this;
        }

        public Shader ClearTags()
        {
            _tags.Clear();

            return this;
        }

        public Shader AddPragmaticDirectives(params string[] directives)
        {
            foreach (var obj in directives)
            {
                _pragmaticDirectives.Add(obj);
            }

            return this;
        }

        public Shader AddToPragmaticDirective(string addition, int indexFromEnd = 1)
        {
            _pragmaticDirectives[^indexFromEnd] += " " + addition;

            return this;
        }

        public Shader SetPragmaticDirective(string directive, int indexFromEnd = 1)
        {
            _pragmaticDirectives[^indexFromEnd] = directive;

            return this;
        }

        public Shader AddStruct(string code)
        {
            if (Regex.IsMatch(code, STRUCT_REGEX))
            {
                var structHeader = Regex.Match(code, STRUCT_REGEX).Value;
                if (!_structs.ContainsKey(structHeader))
                {
                    _structs.Add(structHeader, code);
                }
                else
                {
                    throw new ArgumentException("Could not add struct, one with that name already exists.");
                }
            }
            else
            {
                throw new ArgumentException("Couldn't add struct, could not find a struct in the code.");
            }

            return this;
        }

        public Shader OverrideStruct(string code)
        {
            if (Regex.IsMatch(code, STRUCT_REGEX))
            {
                var structHeader = Regex.Match(code, STRUCT_REGEX).Value;
                if (_structs.ContainsKey(structHeader))
                {
                    _structs[structHeader] = code;
                }
                else
                {
                    throw new ArgumentException("Could not override struct, there is no struct with that name.");
                }
            }
            else
            {
                throw new ArgumentException("Couldn't override struct, could not find a struct in the code.");
            }

            return this;
        }

        public Shader AddFunction(string code, int index = 0)
        {
            if (Regex.IsMatch(code, FUNCTION_SIGNATURE_REGEX))
            {
                var signature = Regex.Match(code, FUNCTION_SIGNATURE_REGEX).Value;

                if (_functions.Contains(signature))
                {
                    throw new ArgumentException("Can't add function, a function with that signature already exists. You can override it with Override().");
                }

                if (code.Contains(signature))
                {
                    code = code.Replace(signature, string.Empty);
                }

                _functions.Insert(index, signature, code);
            }
            else
            {
                throw new ArgumentException("Couldn't find a function signature.");
            }

            return this;
        }

        public Shader OverrideFunction(string code)
        {
            if (Regex.IsMatch(code, FUNCTION_SIGNATURE_REGEX))
            {
                var signature = Regex.Match(code, FUNCTION_SIGNATURE_REGEX).Value;

                if (!_functions.Contains(signature))
                {
                    throw new ArgumentException("Can't override function, a function with that signature doesn't exist.");
                }

                if (code.Contains(signature))
                {
                    code = code.Replace(signature, string.Empty);
                }

                _functions[signature] = code;
            }
            else
            {
                throw new ArgumentException("Couldn't find a function signature.");
            }

            return this;
        }

        public Shader Fallback(string fallback)
        {
            _fallback = fallback;

            return this;
        }

        public Shader Inherit(string newShaderName)
        {
            var newShader = new Shader(newShaderName, Program);
            newShader._shaderProperties.AddRange(_shaderProperties);
            newShader._commands = new List<string>(_commands);
            newShader._pragmaticDirectives = new List<string>(_pragmaticDirectives);
            newShader._fallback = _fallback;
            newShader._tags = new Dictionary<string, string>(_tags);
            newShader._structs = new Dictionary<string, string>(_structs);
            foreach (var func in _functions.Keys)
            {
                newShader._functions.Add(func, _functions[func]);
            }

            return newShader;
        }

        public void SaveAs(string fileName)
        {
            using var stream = new StreamWriter(fileName);

            stream.WriteLine($"Shader \"{Name}\"");
            stream.WriteLine("{");

            stream.WriteLine("\tProperties");
            stream.WriteLine("\t{");

            foreach (var property in _shaderProperties)
            {
                stream.WriteLine($"\t\t{property}");
            }

            stream.WriteLine("\t}");
            stream.WriteLine(string.Empty);
            stream.WriteLine("\tSubShader");
            stream.WriteLine("\t{");
            stream.Write("\t\tTags { ");

            foreach (var tag in _tags)
            {
                stream.Write($"\"{tag.Key}\" = \"{tag.Value}\" ");
            }

            stream.Write("}" + Environment.NewLine);

            stream.WriteLine(string.Empty);

            stream.WriteLine(Program == Program.CGPROGRAM ? "\t\tCGPROGRAM" : "\t\tHLSLPROGRAM");

            stream.WriteLine(string.Empty);

            foreach (var pragma in _pragmaticDirectives)
            {
                stream.WriteLine($"\t\t#pragma {pragma}");
            }

            stream.WriteLine(string.Empty);

            foreach (var st in _structs.Values)
            {
                var stru = InsertTabsAtEndOfLines(st);
                stream.WriteLine($"\t\t{stru}");
                stream.WriteLine(string.Empty);
            }

            foreach (var prop in _shaderProperties)
            {
                if (!((dynamic) prop).GenerateDefinition)
                {
                    continue;
                }

                switch (prop)
                {
                    case ShaderProperty<int> i:
                        stream.WriteLine($"\t\tint {i.PropertyName};");

                        break;
                    case ShaderProperty<float> fl:
                        if (fl.DefaultValue > -2f || fl.DefaultValue < 2f)
                        {
                            stream.WriteLine($"\t\tfixed {fl.PropertyName};");
                        }
                        else if (fl.DefaultValue > -60000 || fl.DefaultValue < 60000)
                        {
                            stream.WriteLine($"\t\thalf {fl.PropertyName};");
                        }
                        else
                        {
                            stream.WriteLine($"\t\tfloat {fl.PropertyName};");
                        }

                        break;
                    case ShaderProperty<Range> range:
                        if (range.DefaultValue.Min > -2f || range.DefaultValue.Max < 2f)
                        {
                            stream.WriteLine($"\t\tfixed {range.PropertyName};");
                        }
                        else if (range.DefaultValue.Min > -60000 || range.DefaultValue.Max < 60000)
                        {
                            stream.WriteLine($"\t\thalf {range.PropertyName};");
                        }
                        else
                        {
                            stream.WriteLine($"\t\tfloat {range.PropertyName};");
                        }

                        break;
                    case ShaderProperty<Texture2D> texture2d:
                        stream.WriteLine($"\t\tsampler2D {texture2d.PropertyName};");
                        break;
                    case ShaderProperty<Texture2DArray> _:
                    case ShaderProperty<Texture3D> _:
                    case ShaderProperty<Cubemap> _:             // TODO
                    case ShaderProperty<CubemapArray> _:
                    case ShaderProperty<Color> _:
                    case ShaderProperty<Vector> _:
                        break;
                }
            }

            stream.WriteLine(string.Empty);

            foreach (var sig in _functions.Keys)
            {
                stream.Write($"\t\t{sig}");

                var code = (string)_functions[sig];
                if (code == null)
                    continue;

                code = InsertTabsAtEndOfLines(code);

                stream.WriteLine($"{code}");
                stream.Write(Environment.NewLine);
            }

            stream.WriteLine(Program == Program.CGPROGRAM ? "\t\tENDCG" : "\t\tENDHLSL");

            stream.WriteLine("\t}");

            stream.WriteLine(string.Empty);

            if (!string.IsNullOrEmpty(_fallback))
            {
                stream.WriteLine($"\tFallback \"{_fallback}\"");
            }

            stream.WriteLine("}");
        }

        private static string InsertTabsAtEndOfLines(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == '\n')
                {
                    input = input.Insert(i + 1, "\t\t");
                }
            }

            return input;
        }
    }
}
