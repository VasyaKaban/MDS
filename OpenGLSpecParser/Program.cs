using System.Text.RegularExpressions;
using System.Xml.Linq;

struct EnumKey
{
    public string Name { get; set; }
    public string? Api { get; set; }
}

struct EnumValue
{
    public string Value { get; set; }
    public string? Type { get; set; }
}

struct CommandKey
{
    public string Name { get; set; }
}

struct CommandArgument
{
    public string Name { get; set; }
    public string Type { get; set; }
}

struct CommandValue
{
    public string ReturnType { get; set; }
    public List<CommandArgument> Arguments { get; set; }
}

/*
Profile=Core|Compatibility
SpecType=OpenGL|OpenGLES
Version=4.5
Extensions=GL_EXT_Something1,GL_EXT_Something2,GL_EXT_Something3,...
 
 */

enum SpecPofile
{
    Core,
    Compatibility
}

enum SpecType
{
    OpenGL
}

struct SpecVersion
{
    public int Major { get; set; }
    public int Minor { get; set; }
}

struct RequiredSpec
{
    public SpecPofile Pofile { get; set; }
    public SpecType SpecificationType { get; set; }
    public SpecVersion Version { get; set; }
    public List<string> Extensions { get; set; }
}

struct FeatureKey
{
    public string Name { get; set; }
}

struct FeatureValue
{
    public string Api { get; set; }
    public SpecVersion Version { get; set; }
    public List<string> Enums { get; set; }
    public List<string> Commands { get; set; }
    public List<string> Types { get; set; }

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
    public List<string> Apis { get; set; }
    public List<string> Enums { get; set; }
    public List<string> Commands { get; set; }
    public List<string> Types { get; set; }
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
                               select
                               (
                                new EnumKey { Name = enumNode.Attribute("name")!.Value, Api = enumNode.Attribute("api")?.Value },
                                new EnumValue { Value = enumNode.Attribute("value")!.Value.ToLower(), Type = enumNode.Attribute("type")?.Value }
                               );

            foreach (var (key, value) in enumElements)
                enums.TryAdd(key, value);

            var commandElements = from commandGroup in registry.Elements("commands")
                                  from commandNode in commandGroup.Elements("command")
                                  select
                                  (
                                    new CommandKey { Name = commandNode.Element("proto")!.Element("name")!.Value },
                                    new CommandValue
                                    {
                                        ReturnType = String.Join(" ", commandNode.Element("proto")!.Nodes().Where((n) => n is XText || (n is XElement elem && elem.Name == "ptype")).Select((n) => n is XText ? n.ToString().Trim() : ((XElement)n).Value)),
                                        Arguments = new List<CommandArgument>(from paramNode in commandNode.Elements("param")
                                                                              select new CommandArgument
                                                                              {
                                                                                  Name = paramNode.Element("name")!.Value,
                                                                                  Type = String.Join(" ", paramNode.Nodes().Where((n) => n is XText || (n is XElement elem && elem.Name == "ptype")).Select((n) => n is XText ? n.ToString().Trim() : ((XElement)n).Value))
                                                                              })
                                    }
                                  );

            var commands = new Dictionary<CommandKey, CommandValue>();

            foreach (var (key, value) in commandElements)
                commands.TryAdd(key, value);

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
                                        Api = featureNode.Attribute("api")!.Value,
                                        Version =
                                        new SpecVersion
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
                                        Types = new List<string>(from requireNode in requireNodes
                                                                 from typeNode in requireNode.Elements("type")
                                                                 select typeNode.Attribute("name")!.Value),
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
                                          Apis = new List<string>(extensionSupportedSpecMatch.Select(m => m.Value)),
                                          Enums = new List<string>(from requireNode in requireNodes
                                                                   from enumNode in requireNode.Elements("enum")
                                                                   select enumNode.Attribute("name")!.Value),
                                          Commands = new List<string>(from requireNode in requireNodes
                                                                      from commandNode in requireNode.Elements("command")
                                                                      select commandNode.Attribute("name")!.Value),
                                          Types = new List<string>(from requireNode in requireNodes
                                                                   from typeNode in requireNode.Elements("type")
                                                                   select typeNode.Attribute("name")!.Value)
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
                                     Data = String.Join("", typeNode.Nodes().Where((n) => n is XText).Select((n) => n.ToString()))
                                 }
                               );

            var types = new Dictionary<TypeKey, TypeValue>();
            foreach (var (key, value) in typeElements)
                types.TryAdd(key, value);


            RequiredSpec spec = new RequiredSpec
            {
                Pofile = SpecPofile.Core,
                SpecificationType = SpecType.OpenGL,
                Version = new SpecVersion { Major = 4, Minor = 5 },
                Extensions = ["GL_S3_s3tc", "GL_ARB_base_instance"]
            };


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

            foreach (var (key, value) in enums)
                Console.WriteLine($"[{(key.Api is null ? "common" : key.Api)}]: {key.Name} = {value.Value} : {(value.Type is null ? "enum" : value.Type)}");

            foreach (var (key, value) in commands)
                Console.WriteLine($"{value.ReturnType} {key.Name}({String.Join(", ", value.Arguments.Select((arg) => String.Concat(arg.Type, " ", arg.Name)))})");
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