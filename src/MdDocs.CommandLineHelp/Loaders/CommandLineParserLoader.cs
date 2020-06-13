﻿using System;
using System.Linq;
using Grynwald.MdDocs.CommandLineHelp.Model2;
using Grynwald.MdDocs.Common;
using Microsoft.Extensions.Logging;
using Mono.Cecil;

namespace Grynwald.MdDocs.CommandLineHelp.Loaders
{
    /// <summary>
    /// Loader for applications based on the <see href="https://www.nuget.org/packages/CommandLineParser/">CommandLineParser</see> package.
    /// </summary>
    public class CommandLineParserLoader : IDocumentationLoader
    {
        private const string s_Hidden = "Hidden";
        private const string s_HelpText = "HelpText";

        private readonly ILogger m_Logger;


        public CommandLineParserLoader(ILogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public ApplicationDocumentation Load(AssemblyDefinition assembly)
        {
            var types = assembly.MainModule.Types.Where(x => !x.IsAbstract);

            if (types.Any(x => x.HasAttribute(CommandLineParserTypeNames.VerbAttributeFullName)))
            {
                m_Logger.LogInformation($"Found a class attributed with '{CommandLineParserTypeNames.VerbAttributeFullName}'. Assuming application has sub-commands");
                return LoadMultiCommandApplication(assembly);
            }
            else
            {
                m_Logger.LogInformation($"Found *no* class attributed with '{CommandLineParserTypeNames.VerbAttributeFullName}'. Assuming application without sub-commands");
                return LoadSingleCommandApplication(assembly);
            }

        }


        private MultiCommandApplicationDocumentation LoadMultiCommandApplication(AssemblyDefinition assembly)
        {
            var name = GetApplicationName(assembly);
            var version = assembly.GetInformationalVersionOrVersion();
            var application = new MultiCommandApplicationDocumentation(name, version);

            LoadCommands(application, assembly);

            return application;
        }

        private SingleCommandApplicationDocumentation LoadSingleCommandApplication(AssemblyDefinition assembly)
        {
            var name = GetApplicationName(assembly);
            var version = assembly.GetInformationalVersionOrVersion();
            var application = new SingleCommandApplicationDocumentation(name, version);

            bool IsCommandLineParameter(PropertyDefinition property)
            {
                return property.HasAttribute(CommandLineParserTypeNames.OptionAttributeFullName) || property.HasAttribute(CommandLineParserTypeNames.ValueAttributeFullName);
            }

            // get all types with at least one property attributed as either [Option] or [Value]
            var optionTypes = assembly.MainModule.Types
                .Where(x => !x.IsAbstract)
                .Where(type => type.Properties.Any(IsCommandLineParameter))
                .ToArray();

            TypeDefinition? optionType = null;
            // no option classes found => return "empty" command (unnamed command without options or values)
            if (optionTypes.Length == 0)
            {
                m_Logger.LogWarning("No option classes found.");
            }
            // use the first option class if multiple candidates were found but log a warning
            else if (optionTypes.Length > 1)
            {
                optionType = optionTypes[0];
                var ignoredTypeNames = optionTypes.Skip(1).Select(x => x.FullName);
                m_Logger.LogWarning(
                    $"Multiple option classes found. Generating documentation for type {optionType.FullName}. " +
                    $"Ignored types: {String.Join(", ", ignoredTypeNames)}");
            }
            // single option class found
            else
            {
                optionType = optionTypes[0];
            }

            if (optionType != null)
            {
                LoadOptions(application, optionType);

                LoadValues(application, optionType);
            }



            return application;
        }

        private string GetApplicationName(AssemblyDefinition definition)
        {
            var name = definition
                .GetAttributeOrDefault(SystemTypeNames.AssemblyTitleAttributeFullName)
                ?.ConstructorArguments?.Single().Value as string;

            if (name == null || String.IsNullOrEmpty(name))
            {
                // no AssemblyTitle specified => return assembly name
                return definition.Name.Name;
            }

            return name;
        }

        private void LoadCommands(MultiCommandApplicationDocumentation application, AssemblyDefinition assembly)
        {
            var commandTypes = assembly.MainModule.Types
                .Where(x => !x.IsAbstract)
                .WithAttribute(CommandLineParserTypeNames.VerbAttributeFullName)
                .Where(x => !x.GetAttribute(CommandLineParserTypeNames.VerbAttributeFullName).GetPropertyValueOrDefault<bool>(s_Hidden));


            foreach (var commandType in commandTypes)
            {
                var verbAttribute = commandType.GetAttribute(CommandLineParserTypeNames.VerbAttributeFullName);

                var name = (string)verbAttribute.ConstructorArguments.First(x => x.Type.FullName == SystemTypeNames.StringFullName).Value;

                var commandDocumentation = application.AddCommand(name);
                commandDocumentation.Description = verbAttribute.GetPropertyValueOrDefault<string>(s_HelpText);

                LoadOptions(commandDocumentation, commandType);

                LoadValues(commandDocumentation, commandType);
            }
        }

        private void LoadOptions(IParameterCollection parameterCollection, TypeDefinition commandType)
        {
            var optionProperties = commandType.GetAllProperties()
                .WithAttribute(CommandLineParserTypeNames.OptionAttributeFullName)
                .Where(x => !x.GetAttribute(CommandLineParserTypeNames.OptionAttributeFullName).GetPropertyValueOrDefault<bool>(s_Hidden));

            foreach (var property in optionProperties)
            {
                var (name, shortName) = GetOptionNames(property);

                // boolean parameters are treated as switch parameters
                if (property.PropertyType.FullName == SystemTypeNames.BooleanFullName)
                {
                    _ = parameterCollection.AddSwitchParameter(name, shortName?.ToString());

                    //TODO: Warning if Required = false, or Default != false
                }
                else
                {
                    _ = parameterCollection.AddNamedParameter(name, shortName?.ToString());
                }
            }

        }

        private (string? name, char? shortName) GetOptionNames(PropertyDefinition definition)
        {
            string? name = default;
            char? shortName = default;
            foreach (var arg in definition.GetAttribute(CommandLineParserTypeNames.OptionAttributeFullName).ConstructorArguments)
            {
                if (arg.Type.FullName == SystemTypeNames.StringFullName)
                {
                    name = (string)arg.Value;
                }
                else if (arg.Type.FullName == SystemTypeNames.CharFullName)
                {
                    shortName = (char)arg.Value;
                }
                else
                {
                    m_Logger.LogWarning($"{definition.FullName}: Unexpected constructor argument of type '{arg.Type.FullName}' in OptionAttribute.");
                }
            }

            return (name, shortName);
        }

        private void LoadValues(IParameterCollection parameterCollection, TypeDefinition commandType)
        {
            var valueProperties = commandType.GetAllProperties()
                .WithAttribute(CommandLineParserTypeNames.ValueAttributeFullName)
                .Where(x => !x.GetAttribute(CommandLineParserTypeNames.ValueAttributeFullName).GetPropertyValueOrDefault<bool>(s_Hidden));

            foreach (var property in valueProperties)
            {
                var valueAttribute = property.GetAttribute(CommandLineParserTypeNames.ValueAttributeFullName);
                var position = (int)valueAttribute.ConstructorArguments.Single().Value;

                _ = parameterCollection.AddPositionalParameter(position);
            }
        }
    }
}
