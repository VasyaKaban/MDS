using System.Text.RegularExpressions;
using System.Xml.Linq;

enum Api
{
    Common,//empty
    GL,//gl
    GLES1,//gles1
    GLES2,//gles2
    GLSC2,//glsc2
}

static class ApiExtensions
{
    extension(Api api)
    {
        public static Api FromString(string? str)
        {
            return str switch
            {
                "gl" => Api.GL,
                "gles1" => Api.GLES1,
                "gles2" => Api.GLES2,
                "glsc2" => Api.GLSC2,
                null => Api.Common,
                _ => Api.Common
            };
        }
    }
}

class Enum
{
    public Dictionary<Api, string> Values { get; }

    public Enum(Dictionary<Api, string> values)
    {
        Values = values;
    }
}

class CommandArgument
{
    public string Name { get; }
    public string Type { get; set; }

    public CommandArgument(string name, string type)
    {
        Name = name;
        Type = type;
    }
}

class Command
{
    public string ReturnType { get; }
    public List<CommandArgument> Arguments { get; }

    public Command(string returnType, List<CommandArgument> arguments)
    {
        ReturnType = returnType;
        Arguments = arguments;
    }
}

class FeatureVersion
{
    public uint Major { get; }
    public uint Minor { get; }

    public FeatureVersion(uint major, uint minor)
    {
        Major = major;
        Minor = minor;
    }

    public ulong Flatten()
    {
        return (((ulong)Major) << 32) | (ulong)Minor;
    }

    public override bool Equals(object? obj)
    {
        if (obj is FeatureVersion version)
            return this == version;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(FeatureVersion v1, FeatureVersion v2)
    {
        return v1.Flatten() == v2.Flatten();
    }

    public static bool operator !=(FeatureVersion v1, FeatureVersion v2)
    {
        return v1.Flatten() != v2.Flatten();
    }

    public static bool operator <(FeatureVersion v1, FeatureVersion v2)
    {
        return v1.Flatten() < v2.Flatten();
    }

    public static bool operator >(FeatureVersion v1, FeatureVersion v2)
    {
        return v1.Flatten() > v2.Flatten();
    }

    public static bool operator <=(FeatureVersion v1, FeatureVersion v2)
    {
        return v1.Flatten() <= v2.Flatten();
    }

    public static bool operator >=(FeatureVersion v1, FeatureVersion v2)
    {
        return v1.Flatten() >= v2.Flatten();
    }
}

class Feature
{
    public Api Api { get; }
    public FeatureVersion Version { get; }
    public List<string> Enums { get; }
    public List<string> Commands { get; }
    //public List<string> Types { get; set; }

    public List<string> CoreProfileRemovedEnums { get; }
    public List<string> CoreProfileRemovedCommands { get; }
    //public List<string> CoreProfileRemovedTypes { get; }

    public Feature(Api api, FeatureVersion version, List<string> enums, List<string> commands, List<string> coreProfileRemovedEnums, List<string> coreProfileRemovedCommands)
    {
        Api = api;
        Version = version;
        Enums = enums;
        Commands = commands;
        CoreProfileRemovedEnums = coreProfileRemovedEnums;
        CoreProfileRemovedCommands = coreProfileRemovedCommands;
    }
}

class Extension
{
    public bool CoreSupported { get; }
    public List<Api> Apis { get; }
    public List<string> Enums { get; }
    public List<string> Commands { get; }
    //public List<string> Types { get; set; }

    public Extension(bool coreSupported, List<Api> apis, List<string> enums, List<string> commands)
    {
        CoreSupported = coreSupported;
        Apis = apis;
        Enums = enums;
        Commands = commands;
    }
}

class Type
{
    public string Data { get; }

    public Type(string data)
    {
        Data = data;
    }
}

struct Spec
{
    public Dictionary<string, Enum> Enums { get; }
    public Dictionary<string, Command> Commands { get; }
    public Dictionary<string, Feature> Features { get; }
    public Dictionary<string, Extension> Extensions { get; }
    public Dictionary<string, Type> Types { get; }

    public Spec()
    {
        Enums = [];
        Commands = [];
        Features = [];
        Extensions = [];
        Types = [];
    }
}

struct LimitedSpec
{
    public readonly Dictionary<string, string> Enums { get; }
    public readonly Dictionary<string, Command> Commands { get; }
    public readonly Dictionary<string, Type> Types { get; }

    public readonly Dictionary<string, Feature> listedFeatures { get; }
    public readonly Dictionary<string, Extension> listedExtensions { get; }

    public LimitedSpec()
    {
        Enums = [];
        Commands = [];
        Types = [];
        listedFeatures = [];
        listedExtensions = [];
    }
}

struct SpecRequirements
{
    public Api Api
    {
        get;
        init
        {
            if (value == Api.Common)
                throw new ArgumentException($"Api field should not be {field.ToString()}");

            field = value;
        }
    }

    public FeatureVersion Version { get; init; }
    public bool UseCompatibilityProfile { get; init; }
    public List<string> EnabledExtensions { get; init; }
}

class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();

        try
        {
            var response = await client.GetAsync("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml");
            string content = await response.Content.ReadAsStringAsync();

            XDocument xmlDoc = XDocument.Parse(content);
            var registry = xmlDoc.Element("registry")!;

            Spec spec = new Spec();

            var enumElements = from enumGroup in registry.Elements("enums")
                               from enumNode in enumGroup.Elements("enum")
                               select
                               (
                                   name: enumNode.Attribute("name")!.Value,
                                   api: Api.FromString(enumNode.Attribute("api")?.Value),
                                   value: enumNode.Attribute("value")!.Value.ToLower()
                               );

            foreach (var (name, api, value) in enumElements)
            {
                var key = name;

                if (spec.Enums.TryGetValue(key, out var enumValue))
                {
                    enumValue.Values.Add(api, value);
                }
                else
                {
                    spec.Enums.Add(key, new Enum
                    (
                        values: new Dictionary<Api, string>
                        {
                            { api, value }
                        }
                    ));
                }
            }

            var commandElements = from commandGroup in registry.Elements("commands")
                                  from commandNode in commandGroup.Elements("command")
                                  select
                                  (
                                      name: commandNode.Element("proto")!.Element("name")!.Value,
                                      value: new Command
                                      (
                                        returnType: String.Join(" ", commandNode.Element("proto")!.Nodes().Where(
                                            (n) => n is XText || (n is XElement elem && elem.Name == "ptype")).Select(
                                            (n) => n is XText ? n.ToString().Trim() : ((XElement)n).Value)),
                                        arguments: new List<CommandArgument>(from paramNode in commandNode.Elements("param")
                                                                             select new CommandArgument
                                                                             (
                                                                                 name: paramNode.Element("name")!.Value,
                                                                                 type: String.Join(" ", paramNode.Nodes().Where(
                                                                                     (n) => n is XText || (n is XElement elem && elem.Name == "ptype")).Select(
                                                                                     (n) => n is XText ? n.ToString().Trim() : ((XElement)n).Value))
                                                                             ))
                                      )
                                  );

            foreach (var (name, value) in commandElements)
            {
                foreach (var arg in value.Arguments)
                {
                    if (arg.Type.Contains("struct"))
                        arg.Type = arg.Type.Replace("struct", "").Replace("*", "").Trim();
                    else if (arg.Type.Contains("const"))
                        arg.Type = arg.Type.Replace("const", "").Trim();
                }

                spec.Commands.TryAdd(name, value);
            }

            var versionRegex = new Regex(@"(\d)\.(\d)");

            var featureElements = from featureNode in registry.Elements("feature")
                                  let versionNumberMatch = versionRegex.Match(featureNode.Attribute("number")!.Value)
                                  let requireNodes = featureNode.Elements("require")
                                  let coreProfileRemoveNodes = featureNode.Elements("remove")
                                  select
                                  (
                                    name: featureNode.Attribute("name")!.Value,
                                    value: new Feature
                                    (
                                        api: Api.FromString(featureNode.Attribute("api")!.Value),
                                        version: new FeatureVersion
                                        (
                                            major: uint.Parse(versionNumberMatch.Groups[1].Value),
                                            minor: uint.Parse(versionNumberMatch.Groups[2].Value)
                                        ),
                                        enums: new List<string>(from requireNode in requireNodes
                                                                from enumNode in requireNode.Elements("enum")
                                                                select enumNode.Attribute("name")!.Value),
                                        commands: new List<string>(from requireNode in requireNodes
                                                                   from commandNode in requireNode.Elements("command")
                                                                   select commandNode.Attribute("name")!.Value),
                                        /*types: new List<string>(from requireNode in requireNodes
                                                                 from typeNode in requireNode.Elements("type")
                                                                 select typeNode.Attribute("name")!.Value),*/
                                        coreProfileRemovedEnums: new List<string>(from coreProfileRemoveNode in coreProfileRemoveNodes
                                                                                  from enumNode in coreProfileRemoveNode.Elements("enum")
                                                                                  select enumNode.Attribute("name")!.Value),
                                        coreProfileRemovedCommands: new List<string>(from coreProfileRemoveNode in coreProfileRemoveNodes
                                                                                     from commandNode in coreProfileRemoveNode.Elements("command")
                                                                                     select commandNode.Attribute("name")!.Value)/*,
                                        coreProfileRemovedTypes: new List<string>(from coreProfileRemoveNode in coreProfileRemoveNodes
                                                                                  from typeNode in coreProfileRemoveNode.Elements("type")
                                                                                  select typeNode.Attribute("name")!.Value),*/

                                    )
                                  );

            foreach (var (key, value) in featureElements)
                spec.Features.TryAdd(key, value);


            var extensionSupportedSpecRegex = new Regex(@"\w+");

            var extensionElements = from extensionNode in registry.Element("extensions")!.Elements("extension")
                                    let extensionSupportedSpecMatch = extensionSupportedSpecRegex.Matches(extensionNode.Attribute("supported")!.Value)
                                    let requireNodes = extensionNode.Elements("require")
                                    select
                                    (
                                      name: extensionNode.Attribute("name")!.Value,
                                      value: new Extension
                                      (
                                          coreSupported: extensionSupportedSpecMatch.Any((api) => api.Value == "glcore"),
                                          apis: new List<Api>(extensionSupportedSpecMatch.Where((m) => m.Value != "glcore").Select((m) => Api.FromString(m.Value))),
                                          enums: new List<string>(from requireNode in requireNodes
                                                                  from enumNode in requireNode.Elements("enum")
                                                                  select enumNode.Attribute("name")!.Value),
                                          commands: new List<string>(from requireNode in requireNodes
                                                                     from commandNode in requireNode.Elements("command")
                                                                     select commandNode.Attribute("name")!.Value)/*,
                                          types: new List<string>(from requireNode in requireNodes
                                                                   from typeNode in requireNode.Elements("type")
                                                                   select typeNode.Attribute("name")!.Value)*/
                                      )
                                    );

            foreach (var (key, value) in extensionElements)
                spec.Extensions.TryAdd(key, value);


            var typeElements = from typeNode in registry.Element("types")!.Elements("type")
                               select
                               (
                                 name: (typeNode.Attribute("name") is not null ? typeNode.Attribute("name")!.Value : typeNode.Element("name")!.Value),
                                 value: new Type
                                 (
                                     data: String.Join("", typeNode.Nodes().Where(
                                         (n) => n is XText).Select(
                                         (n) => n.ToString()))
                                 )
                               );

            var functionPointerRegex = new Regex(@"\(\s*\*\s*\)");
            var functionPointerSplitRegex = new Regex(@"(\w+ ?\w+\s*\*|\w+ )(\w+)");
            var functionPointerReturnTypeRegex = new Regex(@"(\w+)\s*\(\s*\*\s*\)");

            foreach (var (key, value) in typeElements)
            {
                //prepare types
                if (value.Data.Contains("#"))//erase types with defines -> we do not use them at all...
                    continue;

                var clearType = "";
                if (value.Data.Contains("struct"))//replace as void *
                    clearType = "void *";
                else
                    clearType = value.Data.Replace("typedef", "").Replace("khronos_", "").Replace("_t", "").Replace(";", "").Trim();

                if (clearType.Length == 0)
                {
                    clearType = key.Replace("struct", "").Trim();
                    spec.Types.TryAdd(clearType, new Type(data: "void *"));
                }
                else
                {
                    var functionPointerMatch = functionPointerRegex.Match(clearType);
                    if (functionPointerMatch.Success)
                    {
                        var matches = functionPointerSplitRegex.Matches(clearType);
                        var returnType = functionPointerReturnTypeRegex.Match(clearType).Groups[1].Value;

                        clearType = "delegate* unmanaged<";

                        foreach (Match match in matches)
                        {
                            clearType += String.Format($"{match.Groups[1].Value} /*{match.Groups[2].Value}*/, ");
                        }

                        clearType += String.Format($"{returnType}>");//return type
                    }
                    else
                    {
                        if (spec.Types.TryGetValue(clearType, out var tmpValue))
                        {
                            clearType = tmpValue.Data;
                        }
                        else
                        {
                            clearType = clearType switch
                            {
                                "unsigned char" => "byte",
                                "unsigned short" => "ushort",
                                "unsigned int" => "uint",
                                "char" => "sbyte",
                                "int8" => "sbyte",
                                "uint8" => "byte",
                                "int16" => "short",
                                "uint16" => "ushort",
                                "int32" => "int",
                                "uint32" => "uint",
                                "int64" => "long",
                                "uint64" => "ulong",

                                "intptr" => "nint",
                                "ssize" => "nint",
                                _ => clearType
                            };
                        }
                    }

                    spec.Types.TryAdd(key, new Type(data: clearType));
                }
            }

            SpecRequirements requiredSpec = new SpecRequirements
            {
                Api = Api.GL,
                Version = new FeatureVersion(major: 4, minor: 5),
                UseCompatibilityProfile = true,
                EnabledExtensions = ["GL_ARB_base_instance", "GL_ARB_internalformat_query2"]
            };

            LimitedSpec limitedSpec = new LimitedSpec();

            //add all types from spec
            foreach (var (name, type) in spec.Types)
            {
                limitedSpec.Types.Add(name, type);
            }

            //add features
            foreach (var (name, feature) in spec.Features)
            {
                //check feature API
                if (feature.Api != requiredSpec.Api)
                    continue;

                //break if we out of versions
                if (feature.Version > requiredSpec.Version)
                    break;

                limitedSpec.listedFeatures.Add(name, feature);

                //add every enum from current feature
                foreach (var enumName in feature.Enums)
                {
                    Enum enumValue = spec.Enums[enumName];

                    if (enumValue.Values.ContainsKey(Api.Common))
                        limitedSpec.Enums.TryAdd(enumName, enumValue.Values[Api.Common]);
                    else if (enumValue.Values.ContainsKey(requiredSpec.Api))
                        limitedSpec.Enums.TryAdd(enumName, enumValue.Values[requiredSpec.Api]);
                }

                //add every commands from current feature
                foreach (var commandName in feature.Commands)
                {
                    Command commandValue = spec.Commands[commandName];

                    limitedSpec.Commands.TryAdd(commandName, commandValue);
                }

                if (!requiredSpec.UseCompatibilityProfile)
                {
                    //remove all non-core enums
                    foreach (var enumName in feature.CoreProfileRemovedEnums)
                        limitedSpec.Enums.Remove(enumName);

                    //remove all non-core commands
                    foreach (var commandName in feature.CoreProfileRemovedCommands)
                        limitedSpec.Commands.Remove(commandName);
                }
            }

            //add extensions
            foreach (var requiredExt in requiredSpec.EnabledExtensions)
            {
                if (spec.Extensions.TryGetValue(requiredExt, out var extension))
                {
                    if (!extension.Apis.Contains(requiredSpec.Api))
                        throw new Exception($"Extension: {requiredExt} do not implemented in current API set: {requiredSpec.Api}. Only supported APIs: [{String.Join(", ", extension.Apis)}]");

                    if (!requiredSpec.UseCompatibilityProfile && !extension.CoreSupported)
                        throw new Exception($"Extension: {requiredExt} is not supported in Core profile");

                    limitedSpec.listedExtensions.Add(requiredExt, extension);

                    //add every enum from current extension
                    foreach (var enumName in extension.Enums)
                    {
                        Enum enumValue = spec.Enums[enumName];

                        if (enumValue.Values.ContainsKey(Api.Common))
                            limitedSpec.Enums.TryAdd(enumName, enumValue.Values[Api.Common]);
                        else if (enumValue.Values.ContainsKey(requiredSpec.Api))
                            limitedSpec.Enums.TryAdd(enumName, enumValue.Values[requiredSpec.Api]);
                    }

                    //add every commands from current extension
                    foreach (var commandName in extension.Commands)
                    {
                        Command commandValue = spec.Commands[commandName];

                        limitedSpec.Commands.TryAdd(commandName, commandValue);
                    }
                }
                else
                    throw new Exception($"Extension: {requiredExt} has not been found in registry");
            }


            System.Diagnostics.Debugger.Break();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}

/*
Profile=Core|Compatibility
SpecType=OpenGL|OpenGLES
Version=4.5
Extensions=GL_EXT_Something1,GL_EXT_Something2,GL_EXT_Something3,...
 
 */