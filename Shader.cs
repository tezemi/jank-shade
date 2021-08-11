using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using JankShade.Properties;
using Range = JankShade.Properties.Range;

namespace JankShade
{
    public class Shader
    {
        public string Name { get; set; }
        public Program Program { get; set; }
        public string FallbackShader { get; set; }
        public static string IncludeFolder = @"\";
        public static string OutputFolder = @"\";
        private List<string> _commands = new List<string>();
        private List<object> _shaderProperties = new List<object>();
        private List<string> _pragmaticDirectives = new List<string>();
        private Dictionary<string, string> _tags = new Dictionary<string, string>();
        private Dictionary<string, string> _structs = new Dictionary<string, string>();
        private readonly OrderedDictionary _functions = new OrderedDictionary();
        private const string STRUCT_REGEX = "^struct [A-z]*";
        private const string FUNCTION_SIGNATURE_REGEX = @"^[a-z]*[0-9]*(?:\s+\w+)*\s*\(([^),]*)(\s*,\s*[^),]*){0,}\)";

        public Shader(string name, Program program)
        {
            Name = name;
            Program = program;
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

        public Shader AddStruct(string includeFile)
        {
            var code = ReadFile(includeFile);

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

        public Shader OverrideStruct(string includeFile)
        {
            var code = ReadFile(includeFile);

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

        public Shader AddFunction(string includeFile, int index = 0)
        {
            var code = ReadFile(includeFile);

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

        public Shader OverrideFunction(string includeFile)
        {
            var code = ReadFile(includeFile);

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

        public Shader SetFallback(string fallback)
        {
            FallbackShader = fallback;

            return this;
        }

        public Shader Inherit(string newShaderName)
        {
            var newShader = new Shader(newShaderName, Program)
            {
                FallbackShader = FallbackShader,
                _commands = new List<string>(_commands),
                _shaderProperties = new List<object>(_shaderProperties),
                _pragmaticDirectives = new List<string>(_pragmaticDirectives),
                _tags = new Dictionary<string, string>(_tags),
                _structs = new Dictionary<string, string>(_structs)
            };

            foreach (var func in _functions.Keys)
            {
                newShader._functions.Add(func, _functions[func]);
            }

            return newShader;
        }
        
        public void SaveAs(string fileName)
        {
            using var stream = new StreamWriter($@"{OutputFolder}\{fileName}");

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
                if (!((dynamic)prop).GenerateVariable)
                {
                    continue;
                }

                stream.WriteLine($"\t\t{ShaderProperty<object>.GetPropertyDataType(prop)};");
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

            if (!string.IsNullOrEmpty(FallbackShader))
            {
                stream.WriteLine($"\tFallback \"{FallbackShader}\"");
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

        private static string ReadFile(string file)
        {
            return File.ReadAllText($@"{IncludeFolder}\{file}", Encoding.ASCII);
        }
    }
}
