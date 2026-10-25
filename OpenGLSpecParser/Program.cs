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



struct EnumKey
{
    public string Name { get; set; }
}

struct EnumValue
{
    public Dictionary<Api, string> Values { get; set; }
}

struct CommandKey
{
    public string Name { get; set; }
}

class CommandArgument
{
    public string Name { get; set; }
    public string Type { get; set; }
}

struct CommandValue
{
    public string ReturnType { get; set; }
    public List<CommandArgument> Arguments { get; set; }
}

struct FeatureVersion
{
    public int Major { get; set; }
    public int Minor { get; set; }
}

struct FeatureKey
{
    public string Name { get; set; }
}

struct FeatureValue
{
    public Api Api { get; set; }
    public FeatureVersion Version { get; set; }
    public List<string> Enums { get; set; }
    public List<string> Commands { get; set; }
    //public List<string> Types { get; set; }

    public List<string> CoreProfileRemovedEnums { get; set; }
    public List<string> CoreProfileRemovedCommands { get; set; }
    public List<string> CoreProfileRemovedTypes { get; set; }
}

struct ExtensionKey
{
    public string Name { get; set; }
}

struct ExtensionValue
{
    public bool CoreSupported { get; set; }
    public List<Api> Apis { get; set; }
    public List<string> Enums { get; set; }
    public List<string> Commands { get; set; }
    //public List<string> Types { get; set; }
}

struct TypeKey
{
    public string Name { get; set; }
}

struct TypeValue
{
    public string Data { get; set; }
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

            var enums = new Dictionary<EnumKey, EnumValue>();

            var enumElements = from enumGroup in registry.Elements("enums")
                               from enumNode in enumGroup.Elements("enum")
                               select new
                               {
                                   Name = enumNode.Attribute("name")!.Value,
                                   Api = Api.FromString(enumNode.Attribute("api")?.Value),
                                   Value = enumNode.Attribute("value")!.Value.ToLower()
                               };

            foreach (var enumElement in enumElements)
            {
                var key = new EnumKey { Name = enumElement.Name };

                EnumValue value;
                if (enums.TryGetValue(key, out value))
                {
                    value.Values.Add(enumElement.Api, enumElement.Value);
                }
                else
                {
                    enums.Add(key, new EnumValue
                    {
                        Values = new Dictionary<Api, string>
                        {
                            { enumElement.Api, enumElement.Value }
                        }
                    });
                }
            }

            var commandElements = from commandGroup in registry.Elements("commands")
                                  from commandNode in commandGroup.Elements("command")
                                  select
                                  (
                                    new CommandKey
                                    {
                                        Name = commandNode.Element("proto")!.Element("name")!.Value
                                    },
                                    new CommandValue
                                    {
                                        ReturnType = String.Join(" ", commandNode.Element("proto")!.Nodes().Where(
                                            (n) => n is XText || (n is XElement elem && elem.Name == "ptype")).Select(
                                            (n) => n is XText ? n.ToString().Trim() : ((XElement)n).Value)),
                                        Arguments = new List<CommandArgument>(from paramNode in commandNode.Elements("param")
                                                                              select new CommandArgument
                                                                              {
                                                                                  Name = paramNode.Element("name")!.Value,
                                                                                  Type = String.Join(" ", paramNode.Nodes().Where(
                                                                                      (n) => n is XText || (n is XElement elem && elem.Name == "ptype")).Select(
                                                                                      (n) => n is XText ? n.ToString().Trim() : ((XElement)n).Value))
                                                                              })
                                    }
                                  );

            var commands = new Dictionary<CommandKey, CommandValue>();

            foreach (var (key, value) in commandElements)
            {
                foreach (var arg in value.Arguments)
                {
                    if (arg.Type.Contains("struct"))
                        arg.Type = arg.Type.Replace("struct", "").Replace("*", "").Trim();
                    else if (arg.Type.Contains("const"))
                        arg.Type = arg.Type.Replace("const", "").Trim();
                }

                commands.TryAdd(key, value);
            }

            var versionRegex = new Regex(@"(\d)\.(\d)");

            var featureElements = from featureNode in registry.Elements("feature")
                                  let versionNumberMatch = versionRegex.Match(featureNode.Attribute("number")!.Value)
                                  let requireNodes = featureNode.Elements("require")
                                  let coreProfileRemoveNodes = featureNode.Elements("remove")
                                  select
                                  (
                                    new FeatureKey { Name = featureNode.Attribute("name")!.Value },
                                    new FeatureValue
                                    {
                                        Api = Api.FromString(featureNode.Attribute("api")!.Value),
                                        Version = new FeatureVersion
                                        {
                                            Major = int.Parse(versionNumberMatch.Groups[1].Value),
                                            Minor = int.Parse(versionNumberMatch.Groups[2].Value)
                                        },
                                        Enums = new List<string>(from requireNode in requireNodes
                                                                 from enumNode in requireNode.Elements("enum")
                                                                 select enumNode.Attribute("name")!.Value),
                                        Commands = new List<string>(from requireNode in requireNodes
                                                                    from commandNode in requireNode.Elements("command")
                                                                    select commandNode.Attribute("name")!.Value),
                                        /*Types = new List<string>(from requireNode in requireNodes
                                                                 from typeNode in requireNode.Elements("type")
                                                                 select typeNode.Attribute("name")!.Value),*/
                                        CoreProfileRemovedEnums = new List<string>(from coreProfileRemoveNode in coreProfileRemoveNodes
                                                                                   from enumNode in coreProfileRemoveNode.Elements("enum")
                                                                                   select enumNode.Attribute("name")!.Value),
                                        CoreProfileRemovedCommands = new List<string>(from coreProfileRemoveNode in coreProfileRemoveNodes
                                                                                      from commandNode in coreProfileRemoveNode.Elements("command")
                                                                                      select commandNode.Attribute("name")!.Value),
                                        CoreProfileRemovedTypes = new List<string>(from coreProfileRemoveNode in coreProfileRemoveNodes
                                                                                   from typeNode in coreProfileRemoveNode.Elements("type")
                                                                                   select typeNode.Attribute("name")!.Value),

                                    }
                                  );

            var features = new Dictionary<FeatureKey, FeatureValue>();
            foreach (var (key, value) in featureElements)
                features.TryAdd(key, value);


            var extensionSupportedSpecRegex = new Regex(@"\w+");

            var extensionElements = from extensionNode in registry.Element("extensions")!.Elements("extension")
                                    let extensionSupportedSpecMatch = extensionSupportedSpecRegex.Matches(extensionNode.Attribute("supported")!.Value)
                                    let requireNodes = extensionNode.Elements("require")
                                    select
                                    (
                                      new ExtensionKey { Name = extensionNode.Attribute("name")!.Value },
                                      new ExtensionValue
                                      {
                                          CoreSupported = extensionSupportedSpecMatch.Any((api) => api.Value == "glcore"),
                                          Apis = new List<Api>(extensionSupportedSpecMatch.Where((m) => m.Value != "glcore").Select((m) => Api.FromString(m.Value))),
                                          Enums = new List<string>(from requireNode in requireNodes
                                                                   from enumNode in requireNode.Elements("enum")
                                                                   select enumNode.Attribute("name")!.Value),
                                          Commands = new List<string>(from requireNode in requireNodes
                                                                      from commandNode in requireNode.Elements("command")
                                                                      select commandNode.Attribute("name")!.Value)/*,
                                          Types = new List<string>(from requireNode in requireNodes
                                                                   from typeNode in requireNode.Elements("type")
                                                                   select typeNode.Attribute("name")!.Value)*/
                                      }
                                    );

            var extensions = new Dictionary<ExtensionKey, ExtensionValue>();
            foreach (var (key, value) in extensionElements)
                extensions.TryAdd(key, value);


            var typeElements = from typeNode in registry.Element("types")!.Elements("type")
                               select
                               (
                                 new TypeKey { Name = (typeNode.Attribute("name") is not null ? typeNode.Attribute("name")!.Value : typeNode.Element("name")!.Value) },
                                 new TypeValue
                                 {
                                     Data = String.Join("", typeNode.Nodes().Where(
                                         (n) => n is XText).Select(
                                         (n) => n.ToString()))
                                 }
                               );

            var functionPointerRegex = new Regex(@"\(\s*\*\s*\)");
            var functionPointerSplitRegex = new Regex(@"\w+\s*\*?");

            var types = new Dictionary<TypeKey, TypeValue>();
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
                    clearType = key.Name.Replace("struct", "").Trim();
                    types.TryAdd(new TypeKey { Name = clearType }, new TypeValue { Data = "void *" });
                }
                else
                {
                    var functionPointerMatch = functionPointerRegex.Match(clearType);
                    if (functionPointerMatch.Success)
                    {
                        var functionPointerSplitMacthes = functionPointerSplitRegex.Matches(clearType).Where((m) => !m.Value.Contains("const")).ToList();

                        clearType = "delegate* unmanaged<";

                        for (int i = 1; i < functionPointerSplitMacthes.Count; i += 2)
                        {
                            clearType += String.Format($"{functionPointerSplitMacthes[i].Value} /*{functionPointerSplitMacthes[i + 1].Value}*/, ");
                        }

                        clearType += String.Format($"{functionPointerSplitMacthes[0].Value}>");//return type
                    }
                    else
                    {
                        TypeValue tmpValue;
                        if (types.TryGetValue(new TypeKey { Name = clearType }, out tmpValue))
                        {
                            clearType = tmpValue.Data;
                        }
                        else
                        {
                            //Let's target on 64-bit arch
                            clearType = clearType switch
                            {
                                "unsigned char" => "byte",
                                "unsigned short" => "ushort",
                                "unsigned int" => "uint",

                                "char" => "sbyte",
                                "int" => "int",
                                "float" => "float",
                                "double" => "double",
                                "void" => "void",

                                "int8" => "sbyte",
                                "uint8" => "byte",
                                "int16" => "short",
                                "uint16" => "ushort",
                                "int32" => "int",
                                "uint32" => "uint",
                                "int64" => "long",
                                "uint64" => "ulong",

                                "intptr" => "long",
                                "ssize" => "long",
                                _ => clearType
                            };
                        }
                    }

                    types.TryAdd(key, new TypeValue { Data = clearType });
                }
            }

            /*foreach (var (key, value) in enums)
            {
                Console.WriteLine($"Enum: {key.Name}, Values: {String.Join(", ", value.Values.Select((kv) => "[" + kv.Key.ToString() + "]" + " -> " + kv.Value))}");
            }

            foreach (var (key, value) in commands)
            {
                Console.WriteLine($"Command: {value.ReturnType} {key.Name}({String.Join(", ", value.Arguments.Select((a) => a.Type + " " + a.Name))})");
            }

            foreach (var (key, value) in features)
            {
                Console.WriteLine($"Feature: {key.Name}; Api: {value.Api.ToString()}; Version: {value.Version.Major}.{value.Version.Minor}");
                Console.WriteLine($"FeatureEnums({value.Enums.Count}):\n{String.Join("\n", value.Enums)}");
                Console.WriteLine($"FeatureCommands({value.Commands.Count}):\n{String.Join("\n", value.Commands)}");
                Console.WriteLine($"FeatureTypes({value.Types.Count}):\n{String.Join("\n", value.Types)}");
                Console.WriteLine($"FeatureCoreProfileRemovedEnums({value.CoreProfileRemovedEnums.Count}):\n{String.Join("\n", value.CoreProfileRemovedEnums)}");
                Console.WriteLine($"FeatureCoreProfileRemovedCommands({value.CoreProfileRemovedCommands.Count}):\n{String.Join("\n", value.CoreProfileRemovedCommands)}");
                Console.WriteLine($"FeatureCoreProfileRemovedTypes({value.CoreProfileRemovedTypes.Count}):\n{String.Join("\n", value.CoreProfileRemovedTypes)}");
            }

            foreach (var (key, value) in extensions)
            {
                Console.WriteLine($"Extension: {key.Name}; CoreSupported: {value.CoreSupported}; Apis: {String.Join(", ", value.Apis.Select((a) => a.ToString()))}");
                Console.WriteLine($"ExtensionEnums({value.Enums.Count}):\n{String.Join("\n", value.Enums)}");
                Console.WriteLine($"ExtensionCommands({value.Commands.Count}):\n{String.Join("\n", value.Commands)}");
                Console.WriteLine($"ExtensionTypes({value.Types.Count}):\n{String.Join("\n", value.Types)}");
            }*/

            foreach (var (key, value) in types)
            {
                Console.WriteLine($"Type: {key.Name}; Data: {value.Data}");
            }

            System.Diagnostics.Debugger.Break();

#error CONST TO IN???
#error POINTER TYPES TO IN/OUT/REF QUALS
#error VOID* TO IntPtr???
#error MAYBE FOR CALLBACKS USE nint for pointers???
#error CHARS AS MARCHALLING FRO/TO STRING???

            /*RequiredSpec spec = new RequiredSpec
            {
                Pofile = SpecPofile.Core,
                SpecificationType = SpecType.OpenGL,
                Version = new SpecVersion { Major = 4, Minor = 5 },
                Extensions = ["GL_S3_s3tc", "GL_ARB_base_instance"]
            };*/


            /*
             Find features:
                if feature api is current and number is <= current:
                    append all in require tag
                    if profile is Core:
                        remove all in remove tag

             Find extensions:
                if extension in list and is supported by current api and if current profile is compatibility but extension have glcore then emit error:
                    append all in require tag
             */

            //foreach (var (key, value) in enums)
            //    Console.WriteLine($"[{(key.Api is null ? "common" : key.Api)}]: {key.Name} = {value.Value} : {(value.Type is null ? "enum" : value.Type)}");

            //foreach (var (key, value) in commands)
            //    Console.WriteLine($"{value.ReturnType} {key.Name}({String.Join(", ", value.Arguments.Select((arg) => String.Concat(arg.Type, " ", arg.Name)))})");
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