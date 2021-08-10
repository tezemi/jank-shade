using System.Collections.Generic;

namespace JankShade
{
    public class ShaderFunction
    {
        public List<string> Parameters { get; set; }
        public string Code { get; set; }

        public ShaderFunction(params string[] parameters)
        {
            Parameters = new List<string>(parameters);
        }

        public ShaderFunction(ShaderFunction otherToCopy)
        {
            Parameters = new List<string>(otherToCopy.Parameters);
            Code = otherToCopy.Code;
        }
    }
}
