﻿using System;
using System.Collections.Generic;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Grynwald.MarkdownGenerator;
using Grynwald.MdDocs.CommandLineHelp.Model;
using Grynwald.MdDocs.CommandLineHelp.Pages;
using Xunit;

namespace Grynwald.MdDocs.CommandLineHelp.Test.Pages
{
    [Trait("Category", "SkipWhenLiveUnitTesting")]
    [UseReporter(typeof(DiffReporter))]
    public class CommandPageTest
    {
        [Fact]
        public void GetDocument_returns_expected_document_01()
        {
            var model = new CommandDocumentation(new ApplicationDocumentation("test"), "Command1");

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_02()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "Command2",
                helpText: "This is the help text of command 2"
            );

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_03()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "Command2",
                options: new[]
                {
                    new OptionDocumentation("parameter1", helpText: "Help text for parameter 1")
                });

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_04()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "Command2",
                options: new[]
                {
                    new OptionDocumentation("parameter1", required: true),
                    new OptionDocumentation("parameter2"),
                    new OptionDocumentation("parameter3", 'x'),
                    new OptionDocumentation(null, 'y'),
                    new OptionDocumentation("parameter4", hidden: true),
                });

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_05()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "Command2",
                options: new[]
                {
                    new OptionDocumentation(null, 'x'),
                    new OptionDocumentation(null, 'y'),
                });

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_06()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "Command2",
                options: new[]
                {
                    new OptionDocumentation(
                        name: "parameter1",
                        helpText: "Description of parameter 1",
                        @default: "some String"),
                    new OptionDocumentation(
                        name: "parameter2",
                        helpText: "Description of parameter 2",
                        @default: 23),
                });

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_07()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "CommandName",
                options: new[]
                {
                    new OptionDocumentation(
                        name: "parameter1",
                        helpText: "Description of parameter 1",
                        @default: "some String"),
                    new OptionDocumentation(
                        name: "parameter2",
                        helpText: "Description of parameter 2",
                        @default: 23,
                        metaValue: "URI")                   
                },
                values: new[]{
                    new ValueDocumentation(0),
                    new ValueDocumentation(1, metaValue: "INTEGER"),
                    new ValueDocumentation(2, name: "Value3", metaValue: "STRING"),
                });

            Approve(model);
        }

        [Fact]
        public void GetDocument_returns_expected_document_08()
        {
            var model = new CommandDocumentation(
                application: new ApplicationDocumentation("test"),
                name: "CommandName",
                options: new[]
                {
                    new OptionDocumentation(
                        shortName: 'a',
                        helpText: "Description of parameter 1",
                        @default: "some String"),
                    new OptionDocumentation(
                        shortName: 'b',
                        helpText: "Description of parameter 2",
                        @default: 23)
                },
                values: new[]{
                    new ValueDocumentation(0),
                    new ValueDocumentation(1, name: "Value2", required: true),
                    new ValueDocumentation(2, name: "Value3", helpText: "Help text for value 3"),
                    new ValueDocumentation(3, name: "Value4", hidden: true),
                });

            Approve(model);
        }



        //TODO: values + only short name options

        private void Approve(CommandDocumentation model)
        {
            var commandPage = new CommandPage(model);
            var doc = commandPage.GetDocument();

            Assert.NotNull(doc);

            var markdown = doc.ToString();

            var writer = new ApprovalTextWriter(markdown);
            Approvals.Verify(writer, new UnitTestFrameworkNamer(), Approvals.GetReporter());
        }
    }
}
