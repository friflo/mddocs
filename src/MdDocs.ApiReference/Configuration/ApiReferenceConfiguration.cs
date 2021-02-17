﻿using System;
using System.Collections.Generic;
using Grynwald.MdDocs.Common.Configuration;

namespace Grynwald.MdDocs.ApiReference.Configuration
{
    public class ApiReferenceConfiguration
    {
        public enum TemplateName
        {
            Default
        }

        public class DefaultTemplateConfiguration : IConfigurationWithMarkdownPresetSetting
        {
            public bool IncludeAutoGeneratedNotice { get; set; }

            public bool IncludeVersion { get; set; }

            public MarkdownPreset MarkdownPreset { get; set; } = MarkdownPreset.Default;
        }

        public class TemplateConfiguration
        {
            public TemplateName Name { get; set; }

            public DefaultTemplateConfiguration Default { get; set; } = new DefaultTemplateConfiguration();
        }


        [ConvertToFullPath]
        public string OutputPath { get; set; } = "";

        [ConvertToFullPath]
        public string[] AssemblyPaths { get; set; } = Array.Empty<string>();

        public TemplateConfiguration Template { get; set; } = new TemplateConfiguration();
    }
}
