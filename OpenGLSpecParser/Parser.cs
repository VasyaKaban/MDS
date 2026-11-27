using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace OpenGLGenerator
{
    internal readonly union ApiValue(string, Dictionary<string, string>);

    internal record Type(string Name, ApiValue Values);

    internal record Enum(string Name, ApiValue Values, string? Alias);

    internal record CommandArgument(string Type, string Name);

    internal record Command(string Name, string ReturnType, List<CommandArgument> Arguments, string Alias);

    internal record RequireBlock(
        string? Profile,
        string? Api,
        HashSet<string> Types,
        HashSet<string> Enums,
        HashSet<string> Commands);

    internal record RemoveBlock(
        string? Profile,
        string? Api,
        HashSet<string> Types,
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
                    ApiValue apiValue = type.Api switch
                    {
                        null => type.Value,
                        _ => new Dictionary<string, string> { { type.Api!, type.Value } }
                    };

                    spec.Types.Add(type.Name, new Type(type.Name, apiValue));
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
                    ApiValue apiValue = enumObj.Api switch
                    {
                        null => enumObj.Value,
                        _ => new Dictionary<string, string> { { enumObj.Api!, enumObj.Value } }
                    };

                    spec.Enums.Add(enumObj.Name, new Enum(enumObj.Name, apiValue, enumObj.Alias));
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
                           select new
                           {
                               RetunType = string.Concat(commandElement.Element("proto")!.Nodes().Select(n =>
                               {
                                   if (n is XElement e && e.Name != "name")
                                       return e.Value;
                                   else if (n is XText t)
                                       return t.Value;
                                   else
                                       return "";
                               })),
                               Name = commandElement.Element("proto")!.Element("name")!.Value,
                               Alias = commandElement.Element("alias")?.Value,
                               Params = from paramElement in commandElement.Elements("param")
                                        select new
                                        {
                                            Name = paramElement.Element("name")!.Value,
                                            Type = string.Concat(paramElement.Nodes().Select(n =>
                                            {
                                                if (n is XElement e && e.Name != "name")
                                                    return e.Value;
                                                else if (n is XText t)
                                                    return t.Value;
                                                else
                                                    return "";
                                            }))
                                        }
                           };

            foreach (var command in commands)
            {
                spec.Commands.Add(
                    command.Name,
                    new Command(
                        command.Name,
                        command.RetunType,
                        command.Params.Select(p => new CommandArgument(p.Type, p.Name)).ToList(),
                        command.Alias));
            }

            var versionNumberRegex = new Regex(@"(\d+)\.(\d+)");

            var features = from featureElement in registryElement.Elements("feature")
                           let number = versionNumberRegex.Match(featureElement.Attribute("number")!.Value)
                           select new
                           {
                               Name = featureElement.Attribute("name")!.Value,
                               Api = featureElement.Attribute("api")!.Value,
                               MajorVersion = int.Parse(number.Groups[1].ToString()),
                               MinorVersion = int.Parse(number.Groups[2].ToString()),
                               RequireBlocks = from requireElement in featureElement.Elements("require")
                                               select new
                                               {
                                                   Profile = requireElement.Attribute("profile")?.Value,
                                                   Api = requireElement.Attribute("api")?.Value,
                                                   Types = requireElement.Elements("type").Select(e => e.Attribute("name")!.Value),
                                                   Enums = requireElement.Elements("enum").Select(e => e.Attribute("name")!.Value),
                                                   Commands = requireElement.Elements("command").Select(e => e.Attribute("name")!.Value)
                                               },
                               RemoveBlocks = from removeElement in featureElement.Elements("remove")
                                              select new
                                              {
                                                  Profile = removeElement.Attribute("profile")?.Value,
                                                  Api = removeElement.Attribute("api")?.Value,
                                                  Types = removeElement.Elements("type").Select(e => e.Attribute("name")!.Value),
                                                  Enums = removeElement.Elements("enum").Select(e => e.Attribute("name")!.Value),
                                                  Commands = removeElement.Elements("command").Select(e => e.Attribute("name")!.Value)
                                              }
                           };

            foreach (var feature in features)
            {
                spec.Features.Add(
                    feature.Name,
                    new Feature(
                        feature.Name,
                        feature.Api,
                        feature.MajorVersion,
                        feature.MinorVersion,
                        feature.RequireBlocks.Select(r =>
                        {
                            return new RequireBlock(
                                r.Profile,
                                r.Api,
                                r.Types.ToHashSet(),
                                r.Enums.ToHashSet(),
                                r.Commands.ToHashSet());
                        }).ToList(),
                        feature.RemoveBlocks.Select(r =>
                        {
                            return new RemoveBlock(
                                r.Profile,
                                r.Api,
                                r.Types.ToHashSet(),
                                r.Enums.ToHashSet(),
                                r.Commands.ToHashSet());
                        }).ToList()));
            }

            var extensions = from extensionsElement in registryElement.Elements("extensions")
                             from extensionElement in extensionsElement.Elements("extension")
                             select new
                             {
                                 Name = extensionElement.Attribute("name")!.Value,
                                 SupportedApis = extensionElement.Attribute("supported")!.Value,
                                 RequireBlocks = from requireElement in extensionElement.Elements("require")
                                                 select new
                                                 {
                                                     Profile = requireElement.Attribute("profile")?.Value,
                                                     Api = requireElement.Attribute("api")?.Value,
                                                     Types = requireElement.Elements("type").Select(e => e.Attribute("name")!.Value),
                                                     Enums = requireElement.Elements("enum").Select(e => e.Attribute("name")!.Value),
                                                     Commands = requireElement.Elements("command").Select(e => e.Attribute("name")!.Value)
                                                 },
                                 RemoveBlocks = from removeElement in extensionElement.Elements("remove")
                                                select new
                                                {
                                                    Profile = removeElement.Attribute("profile")?.Value,
                                                    Api = removeElement.Attribute("api")?.Value,
                                                    Types = removeElement.Elements("type").Select(e => e.Attribute("name")!.Value),
                                                    Enums = removeElement.Elements("enum").Select(e => e.Attribute("name")!.Value),
                                                    Commands = removeElement.Elements("command").Select(e => e.Attribute("name")!.Value)
                                                }
                             };

            foreach (var extension in extensions)
            {
                spec.Extensions.Add(
                    extension.Name,
                    new Extension(
                        extension.Name,
                        new Regex(extension.SupportedApis),
                        extension.RequireBlocks.Select(r =>
                        {
                            return new RequireBlock(
                                r.Profile,
                                r.Api,
                                r.Types.ToHashSet(),
                                r.Enums.ToHashSet(),
                                r.Commands.ToHashSet());
                        }).ToList(),
                        extension.RemoveBlocks.Select(r =>
                        {
                            return new RemoveBlock(
                                r.Profile,
                                r.Api,
                                r.Types.ToHashSet(),
                                r.Enums.ToHashSet(),
                                r.Commands.ToHashSet());
                        }).ToList()));
            }

            return spec;
        }
    }
}