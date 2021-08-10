
namespace JankShade
{
    public class SurfaceFunction : ShaderFunction
    {
        public LightingModel LightingModel { get; set; }

        public SurfaceFunction(LightingModel lightingModel, params string[] parameters) : base(parameters)
        {
            LightingModel = lightingModel;
        }

        public SurfaceFunction(SurfaceFunction otherToCopy) : base(otherToCopy)
        {
            LightingModel = otherToCopy.LightingModel;
        }
    }
}
