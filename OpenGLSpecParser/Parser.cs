using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpenGLGenerator
{
    internal readonly union ApiValue(string, Dictionary<string, string>);

    internal record Type(string Name, ApiValue Values);

    internal record Enum(string Name, ApiValue Values, string? Alias);

    internal record CommandArgument(string Type, string Name);

    internal record Command(string Name, string ReturnType, List<CommandArgument> Arguments, string? Alias);

    internal record RequireBlock(
        string? Profile,
        string? Api,
        //HashSet<string> Types,
        HashSet<string> Enums,
        HashSet<string> Commands);

    internal record RemoveBlock(
        string? Profile,
        string? Api,
        //HashSet<string> Types,
        HashSet<string> Enums,
        HashSet<string> Commands);

    internal record Feature(
        string Name,
        string Api,
        int MajorVersion,
        int MinorVersion,
        List<RequireBlock> Requires,
        List<RemoveBlock> Removes);

    internal record Extension(
        string Name,
        Regex SupportedApis,
        List<RequireBlock> Requires,
        List<RemoveBlock> Removes);

    internal class Spec
    {
        internal Dictionary<string, Type> Types { get; } = [];
        internal Dictionary<string, Enum> Enums { get; } = [];
        internal Dictionary<string, Command> Commands { get; } = [];
        internal Dictionary<string, Feature> Features { get; } = [];
        internal Dictionary<string, Extension> Extensions { get; } = [];
    }

    internal static class Parser
    {
        internal static Spec Parse(string data)
        {
            Spec spec = new Spec();

            XDocument specDocument = XDocument.Parse(data);
            XElement registryElement = specDocument.Root!;

            var types = from typesElement in registryElement.Elements("types")
                        from typeElement in typesElement.Elements("type")
                        select new
                        {
                            Name = typeElement.Attribute("name")?.Value ?? typeElement.Element("name")!.Value,
                            Api = typeElement.Attribute("api")?.Value,
                            Value = string.Concat(typeElement.Nodes().OfType<XText>())
                        };

            foreach (var type in types)
            {
                if (!spec.Types.ContainsKey(type.Name))
                {
                    spec.Types.Add(type.Name, new Type(type.Name, CreateApiValue(type.Api, type.Value)));
                }
                else
                {
                    if (spec.Types[type.Name].Values is Dictionary<string, string> dict)
                    {
                        dict.Add(type.Api!, type.Value);
                    }
                }
            }

            var enums = from enumsElement in registryElement.Elements("enums")
                        from enumElement in enumsElement.Elements("enum")
                        select new
                        {
                            Name = enumElement.Attribute("name")!.Value,
                            Api = enumElement.Attribute("api")?.Value,
                            Value = enumElement.Attribute("value")!.Value,
                            Alias = enumElement.Attribute("alias")?.Value,
                        };

            foreach (var enumObj in enums)
            {
                if (!spec.Enums.ContainsKey(enumObj.Name))
                {
                    spec.Enums.Add(enumObj.Name, new Enum(enumObj.Name, CreateApiValue(enumObj.Api, enumObj.Value), enumObj.Alias));
                }
                else
                {
                    if (spec.Enums[enumObj.Name].Values is Dictionary<string, string> dict)
                    {
                        dict.Add(enumObj.Api!, enumObj.Value);
                    }
                }
            }

            var commands = from commandsElement in registryElement.Elements("commands")
                           from commandElement in commandsElement.Elements("command")
                           let protoElement = commandElement.Element("proto")!
                           select new Command(
                               Name: protoElement.Element("name")!.Value,
                               ReturnType: GetElementTypeWithoutName(protoElement),
                               Arguments: (from paramElement in commandElement.Elements("param")
                                           select new CommandArgument(
                                               Name: paramElement.Element("name")!.Value,
                                               Type: GetElementTypeWithoutName(paramElement))).ToList(),
                               Alias: commandElement.Element("alias")?.Value);

            foreach (var command in commands)
                spec.Commands.Add(command.Name, command);

            var versionNumberRegex = new Regex(@"(\d+)\.(\d+)");

            var features = from featureElement in registryElement.Elements("feature")
                           let number = versionNumberRegex.Match(featureElement.Attribute("number")!.Value)
                           select new Feature(
                               Name: featureElement.Attribute("name")!.Value,
                               Api: featureElement.Attribute("api")!.Value,
                               MajorVersion: int.Parse(number.Groups[1].ToString()),
                               MinorVersion: int.Parse(number.Groups[2].ToString()),
                               Requires: (from requireElement in featureElement.Elements("require")
                                          select GetElementRequireBlock(requireElement)).ToList(),
                               Removes: (from removeElement in featureElement.Elements("remove")
                                         select GetElementRemoveBlock(removeElement)).ToList());

            foreach (var feature in features)
                spec.Features.Add(feature.Name, feature);

            var extensions = from extensionsElement in registryElement.Elements("extensions")
                             from extensionElement in extensionsElement.Elements("extension")
                             select new Extension(
                                 Name: extensionElement.Attribute("name")!.Value,
                                 SupportedApis: new Regex(extensionElement.Attribute("supported")!.Value),
                                 Requires: (from requireElement in extensionElement.Elements("require")
                                            select GetElementRequireBlock(requireElement)).ToList(),
                                 Removes: (from removeElement in extensionElement.Elements("remove")
                                           select GetElementRemoveBlock(removeElement)).ToList());

            foreach (var extension in extensions)
                spec.Extensions.Add(extension.Name, extension);

            return spec;
        }

        static ApiValue CreateApiValue(string? api, string value)
        {
            ApiValue apiValue = api switch
            {
                null => value,
                _ => new Dictionary<string, string> { { api!, value } }
            };

            return apiValue;
        }

        static string GetElementTypeWithoutName(XElement element)
        {
            return string.Concat(element.Nodes().Select(n =>
            {
                if (n is XElement e && e.Name != "name")
                    return e.Value;
                else if (n is XText t)
                    return t.Value;
                else
                    return "";
            }));
        }

        static RequireBlock GetElementRequireBlock(XElement element)
        {
            return new RequireBlock(
                Profile: element.Attribute("profile")?.Value,
                Api: element.Attribute("api")?.Value,
                //Types: element.Elements("type").Select(e => e.Attribute("name")!.Value).ToHashSet(),
                Enums: element.Elements("enum").Select(e => e.Attribute("name")!.Value).ToHashSet(),
                Commands: element.Elements("command").Select(e => e.Attribute("name")!.Value).ToHashSet());
        }

        static RemoveBlock GetElementRemoveBlock(XElement element)
        {
            return new RemoveBlock(
                Profile: element.Attribute("profile")?.Value,
                Api: element.Attribute("api")?.Value,
                //Types: element.Elements("type").Select(e => e.Attribute("name")!.Value).ToHashSet(),
                Enums: element.Elements("enum").Select(e => e.Attribute("name")!.Value).ToHashSet(),
                Commands: element.Elements("command").Select(e => e.Attribute("name")!.Value).ToHashSet());
        }
    }
}