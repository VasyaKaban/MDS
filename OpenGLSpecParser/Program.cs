using System.Diagnostics;

namespace OpenGLGenerator
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            using HttpClient client = new HttpClient();

            string glSpecData = await client.GetStringAsync("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml");
            var glSpec = Parser.Parse(glSpecData);

            Debugger.Break();

            /*
             {
                Api = GL -> api="gl"
                GLProfile = Core -> supported="glcore"; profile="core"; remove="core"
                Version = 4.6
                Extensions = [GL_AMD_depth_clamp_separate, GL_AMD_gpu_shader_int16]
             },
             {
                Api = GL -> api="gl"
                GLProfile = Compatibility -> supported="gl"
                Version = 4.6
                Extensions = [GL_AMD_depth_clamp_separate, GL_AMD_gpu_shader_int16]
             },
             */
        }
    }
}