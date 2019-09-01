﻿using System;
using System.Linq;
using System.Text;
using Grynwald.MarkdownGenerator;
using Grynwald.MdDocs.CommandLineHelp.Model;

namespace Grynwald.MdDocs.CommandLineHelp.Pages
{
    public class CommandPage : IDocument
    {
        private readonly CommandDocumentation m_Command;


        public CommandPage(CommandDocumentation model)
        {
            m_Command = model ?? throw new ArgumentNullException(nameof(model));
        }


        public void Save(string path) => GetDocument().Save(path);


        internal MdDocument GetDocument()
        {
            var document = new MdDocument();

            // Heading
            document.Root.Add(new MdHeading(1, new MdCompositeSpan(new MdCodeSpan(m_Command.Name), " Command")));

            // Help text
            if (!String.IsNullOrEmpty(m_Command.HelpText))
            {
                document.Root.Add(new MdParagraph(m_Command.HelpText));
            }

            document.Root.Add(GetUsageSection());

            // Parameters
            if (m_Command.Parameters.Any(x => !x.Hidden))
            {
                document.Root.Add(GetParametersSection());
            }

            return document;
        }


        private MdContainerBlock GetParametersSection()
        {
            var block = new MdContainerBlock
            {
                new MdHeading(2, "Parameters"),
                GetParametersTable()
            };

            var sections = Enumerable.Concat(
                m_Command.Values.Where(x => !x.Hidden).Select(GetParameterSection),
                m_Command.Options.Where(x => !x.Hidden).Select(GetParameterSection)
            ).ToArray();


            var first = true;
            foreach (var section in sections)
            {
                if (sections.Length > 1 && !first)
                {
                    block.Add(new MdThematicBreak());
                }

                block.Add(section);
                first = false;
            }

            return block;
        }

        //TODO: Add Required true/false
        private MdTable GetParametersTable()
        {
            MdSpan GetLinkOrEmptySpan(string text, string uri, bool italic = false) =>
                String.IsNullOrEmpty(text) ? (MdSpan)MdEmptySpan.Instance : new MdLinkSpan(italic ? new MdEmphasisSpan(text) : (MdSpan)text, uri);


            var addNameColumn = m_Command.Options.Any(x => !x.Hidden && !String.IsNullOrEmpty(x.Name));
            var addShortNameColumn = m_Command.Options.Any(x => !x.Hidden && x.ShortName != null);
            var addPositionColumn = m_Command.Values.Count > 0;

            var headerRow = new MdTableRow();

            headerRow.AddIf(addPositionColumn, "Position");
            headerRow.AddIf(addNameColumn, "Name");
            headerRow.AddIf(addShortNameColumn && !addNameColumn, "Name");
            headerRow.AddIf(addShortNameColumn && addNameColumn, "Short Name");

            headerRow.Add("Description");

            var table = new MdTable(headerRow);

            foreach (var value in m_Command.Values.Where(x => !x.Hidden).OrderBy(x => x.Index))
            {
                var anchor = "#" + GetHeading(value).Anchor;

                var row = new MdTableRow();

                row.Add(value.Index.ToString());
                row.AddIf(addNameColumn, GetLinkOrEmptySpan(value.Name, anchor, italic: true));

                row.AddIf(addShortNameColumn && !addNameColumn, GetLinkOrEmptySpan(value.Name, anchor, italic: true));
                row.AddIf(addShortNameColumn && addNameColumn, MdEmptySpan.Instance);
                
                row.Add(value.HelpText ?? "");

                table.Add(row);
            }

            foreach (var option in m_Command.Options.Where(x => !x.Hidden))
            {
                var anchor = "#" + GetHeading(option).Anchor;

                var row = new MdTableRow();
                
                row.AddIf(addPositionColumn, MdEmptySpan.Instance);
                row.AddIf(addNameColumn, GetLinkOrEmptySpan(option.Name, anchor));                    
                row.AddIf(addShortNameColumn, GetLinkOrEmptySpan(option.ShortName?.ToString(), anchor));
                row.Add(option.HelpText ?? "");

                table.Add(row);
            }

            return table;
        }
        
        private MdContainerBlock GetParameterSection(OptionDocumentation option)
        {
            var block = new MdContainerBlock
            {
                GetHeading(option)
            };

            if (!String.IsNullOrEmpty(option.HelpText))
            {
                block.Add(new MdParagraph(option.HelpText));
            }

            if (option.Default != null)
            {
                block.Add(new MdParagraph(
                    new MdStrongEmphasisSpan("Default value:"),
                    " ",
                    new MdCodeSpan(Convert.ToString(option.Default))
                ));
            }

            return block;
        }

        private MdContainerBlock GetParameterSection(ValueDocumentation value)
        {
            var block = new MdContainerBlock
            {
                GetHeading(value)
            };

            if (!String.IsNullOrEmpty(value.HelpText))
            {
                block.Add(new MdParagraph(value.HelpText));
            }

            if (value.Default != null)
            {
                block.Add(new MdParagraph(
                    new MdStrongEmphasisSpan("Default value:"),
                    " ",
                    new MdCodeSpan(Convert.ToString(value.Default))
                ));
            }

            return block;
        }

        private static MdHeading GetHeading(OptionDocumentation option)
        {
            var name = option.Name ?? option.ShortName.ToString();
            return new MdHeading(3, new MdCompositeSpan(new MdCodeSpan(name), " Parameter"));
        }

        private static MdHeading GetHeading(ValueDocumentation value)
        {
            if (!String.IsNullOrEmpty(value.Name))
            {
                return new MdHeading(3, new MdCompositeSpan(new MdCodeSpan(value.Name), $" Parameter (Position {value.Index})"));
            }
            else
            {
                //TODO: Find a better heading
                return new MdHeading(3, new MdCompositeSpan($"Parameter (Position {value.Index})"));
            }
        }

        private MdContainerBlock GetUsageSection()
        {
            var block = new MdContainerBlock(new MdHeading(2, "Usage"));

            var usageBuilder = new StringBuilder();
            usageBuilder.Append(m_Command.Application.Name);
            usageBuilder.Append(" ");
            usageBuilder.Append(m_Command.Name);
            usageBuilder.Append(" ");

            var prefixLength = usageBuilder.Length;

            //TODO: values
            foreach(var value in m_Command.Values.Where(x => !x.Hidden).OrderBy(x => x.Index))
            {
                if (!value.Required)
                {
                    usageBuilder.Append("[");
                }

                usageBuilder.Append("<");
                if(!String.IsNullOrEmpty(value.Name))
                {
                    usageBuilder.Append(value.Name);
                }
                else
                {
                    usageBuilder.Append("VALUE");
                }

                if(!String.IsNullOrEmpty(value.MetaValue))
                {
                    usageBuilder.Append(":");
                    usageBuilder.Append(value.MetaValue);
                }


                usageBuilder.Append(">");


                if (!value.Required)
                {
                    usageBuilder.Append("]");
                }

                usageBuilder.AppendLine();
                usageBuilder.Append(' ', prefixLength);
            }

            foreach(var option in m_Command.Options.Where(x => !x.Hidden))
            {
                if(!option.Required)
                {
                    usageBuilder.Append("[");
                }

                // short and long name
                if(!String.IsNullOrEmpty(option.Name) && option.ShortName != null)
                {
                    usageBuilder.Append($"--{option.Name}|-{option.ShortName}");
                }
                // long name only
                else if(!String.IsNullOrEmpty(option.Name) && option.ShortName == null)
                {
                    usageBuilder.Append($"--{option.Name}");
                }
                //short name only
                else if (String.IsNullOrEmpty(option.Name) && option.ShortName != null)
                {
                    usageBuilder.Append($"-{option.ShortName}");
                }
                else
                {
                    throw new InvalidOperationException();
                }


                usageBuilder.Append(" <");
                if(!String.IsNullOrEmpty(option.MetaValue))
                {
                    usageBuilder.Append(option.MetaValue);
                }
                else
                {
                    usageBuilder.Append("VALUE");
                }
                usageBuilder.Append(">");

                if (!option.Required)
                {
                    usageBuilder.Append("]");
                }

                usageBuilder.AppendLine();
                usageBuilder.Append(' ', prefixLength);
            }

            block.Add(new MdCodeBlock(usageBuilder.ToString().TrimEnd()));


            return block;
        }
    }
}
