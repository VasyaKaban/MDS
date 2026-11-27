using System.Diagnostics;

namespace OpenGLGenerator
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            using HttpClient client = new HttpClient();

            /*string glSpecData = await client.GetStringAsync("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml");
            var glSpec = Parser.Parse(glSpecData);

            var trimInfo = new TrimmerInfo("gl", "core", 4, 6, ["GL_AMD_depth_clamp_separate", "GL_AMD_gpu_shader_int16"]);

            var trimmedSpec = Trimmer.Trim(glSpec, trimInfo);*/

            string wglSpecData = await client.GetStringAsync("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/wgl.xml");
            var wglSpec = Parser.Parse(wglSpecData);

            var trimInfo = new TrimmerInfo("wgl", null, 1, 0, []);

            var trimmedSpec = Trimmer.Trim(wglSpec, trimInfo);

            Debugger.Break();
        }
    }
}