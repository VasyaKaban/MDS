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
        int MajorVersion,
        int MinorVersion,
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

            //Explicitly add all types. Although require and remove blocks have <type> elements they do not use them at all
            foreach (var (key, type) in spec.Types)
            {
                trimSpec.Types.Add(key, type);
            }

            foreach (var (_, feature) in spec.Features.Where(f => f.Value.Api == info.Api))
            {
                if (!(feature.MajorVersion <= info.MajorVersion && feature.MinorVersion <= info.MinorVersion))
                    continue;

                foreach (var req in feature.Requires)
                {
                    //skip Api check
                    //but check profile
                    if (requireProfile is null || req.Profile is null || req.Profile == requireProfile)
                    {
                        //insert into trimSpec
                        ProcessRequireBlock(info.Api, spec, trimSpec, req);
                    }
                }

                foreach (var rem in feature.Removes)
                {
                    //skip Api check
                    //but check profile
                    if (requireProfile is null || rem.Profile is null || rem.Profile == requireProfile)
                    {
                        //remove from trimSpec
                        ProcessRemoveBlock(spec, trimSpec, rem);
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
                        ProcessRequireBlock(info.Api, spec, trimSpec, req);
                    }
                }

                foreach (var rem in extension.Removes)
                {
                    if ((rem.Api is null || rem.Api == info.Api) && (requireProfile is null || rem.Profile is null || rem.Profile == requireProfile))
                    {
                        //remove from trimSpec
                        ProcessRemoveBlock(spec, trimSpec, rem);
                    }
                }
            }

            return trimSpec;
        }

        private static void ProcessRequireBlock(string api, Spec spec, TrimmedSpec trimSpec, RequireBlock req)
        {
            /*foreach (string key in req.Types)
            {
                Type type = spec.Types[key];
                string value = type.Values switch
                {
                    string singleValue => singleValue,
                    Dictionary<string, string> dictValue => dictValue[api]
                };

                trimSpec.Types.TryAdd(key, new Type(key, value));
            }*/

            foreach (string key in req.Enums)
            {
                Enum enumObj = spec.Enums[key];
                string value = enumObj.Values switch
                {
                    string singleValue => singleValue,
                    Dictionary<string, string> dictValue => dictValue[api]
                };

                trimSpec.Enums.TryAdd(key, new Enum(key, value, enumObj.Alias));
            }

            foreach (string key in req.Commands)
            {
                Command command = spec.Commands[key];

                trimSpec.Commands.TryAdd(key, command);
            }
        }

        private static void ProcessRemoveBlock(Spec spec, TrimmedSpec trimSpec, RemoveBlock req)
        {
            //foreach (string key in req.Types)
            //    trimSpec.Types.Remove(key);

            foreach (string key in req.Enums)
                trimSpec.Enums.Remove(key);

            foreach (string key in req.Commands)
                trimSpec.Commands.Remove(key);
        }
    }
}
