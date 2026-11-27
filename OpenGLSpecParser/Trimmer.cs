using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGLGenerator
{
    internal class TrimmedSpec
    {
        internal Dictionary<string, Type> Types { get; } = [];
        internal Dictionary<string, Enum> Enums { get; } = [];
        internal Dictionary<string, Command> Commands { get; } = [];
    }

    internal record TrimmerInfo(
        string Api,
        string? ProfilePostfix,
        int MinorVersion,
        int MajorVersion,
        List<string> Extensions);

    /*
    {
        Api = GL -> api="gl"
        ProfilePostfix = core -> supported="glcore"; profile="core"|null; remove="core"|null
        Version = 4.6
        Extensions = [GL_AMD_depth_clamp_separate, GL_AMD_gpu_shader_int16]
    },
    {
        Api = GL -> api="gl"
        ProfilePostfix = null -> supported="gl"
        Version = 4.6
        Extensions = [GL_AMD_depth_clamp_separate, GL_AMD_gpu_shader_int16]
    },
    */

    internal class Trimmer
    {
        internal static TrimmedSpec Trim(Spec spec, TrimmerInfo info)
        {
            TrimmedSpec trimSpec = new TrimmedSpec();

            string supported = info.Api + info.ProfilePostfix ?? "";
            string? requireProfile = info.ProfilePostfix;
            string? removeProfile = info.ProfilePostfix;

            foreach (var (_, feature) in spec.Features.Where(f => f.Value.Api == info.Api))
            {
                if (!(feature.MajorVersion <= info.MajorVersion && feature.MinorVersion <= info.MinorVersion))
                    continue;

                foreach(var req in feature.Requires)
                {
                    //skip Api check
                    //but check profile
                    if(requireProfile is null || req.Profile is null || req.Profile == requireProfile)
                    {
                        //insert into trimSpec
                    }
                }

                foreach (var rem in feature.Removes)
                {
                    //skip Api check
                    //but check profile
                    if (requireProfile is null || rem.Profile is null || rem.Profile == requireProfile)
                    {
                        //remove from trimSpec
                    }
                }
            }

            foreach (var extensionName in info.Extensions)
            {
                if (!spec.Extensions.TryGetValue(extensionName, out var extension))
                    throw new Exception($"Extension: {extensionName} not found in registry");//To log

                if (!extension.SupportedApis.IsMatch(supported))
                    throw new Exception($"Extension: {extensionName} is not supported by the desired API set: {supported}");//To log

                foreach (var req in extension.Requires)
                {
                    if ((req.Api is null || req.Api == info.Api) && (requireProfile is null || req.Profile is null || req.Profile == requireProfile))
                    {
                        //insert into trimSpec
                    }
                }

                foreach (var rem in extension.Removes)
                {
                    if ((rem.Api is null || rem.Api == info.Api) && (requireProfile is null || rem.Profile is null || rem.Profile == requireProfile))
                    {
                        //remove from trimSpec
                    }
                }
            }

            return trimSpec;
        }
    }
}
