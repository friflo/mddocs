﻿using System.IO;
using Grynwald.MarkdownGenerator;
using Grynwald.MdDocs.ApiReference.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.MdDocs.ApiReference.Pages
{
    internal class IndexerPage : OverloadableMemberPage<IndexerDocumentation, IndexerOverloadDocumentation>
    {
        public override string RelativeOutputPath { get; }

        public override OutputPath OutputPath { get; }


        public IndexerPage(ILinkProvider linkProvider, PageFactory pageFactory, string rootOutputPath, IndexerDocumentation model, ILogger logger)
            : base(linkProvider, pageFactory, rootOutputPath, model, logger)
        {
            RelativeOutputPath = Path.Combine(GetTypeDirRelative(Model.TypeDocumentation), "Indexers", $"{Model.Name}.md");
            OutputPath = new OutputPath(GetTypeDir(Model.TypeDocumentation), "Indexers", $"{Model.Name}.md");
        }


        protected override MdHeading GetPageHeading() =>
            new MdHeading($"{Model.TypeDocumentation.DisplayName}.{Model.Name} Indexer", 1);

        protected override void AddParametersSubSection(MdContainerBlock block, IndexerOverloadDocumentation overload, int headingLevel)
        {
            base.AddParametersSubSection(block, overload, headingLevel);
            AddValueSubSection(block, overload, headingLevel);
        }

        protected virtual void AddValueSubSection(MdContainerBlock block, IndexerOverloadDocumentation overload, int headingLevel)
        {
            block.Add(new MdHeading("Indexer Value", headingLevel));
            block.Add(
                GetMdParagraph(overload.Type)
            );

            if (overload.Value != null)
            {
                block.Add(ConvertToBlock(overload.Value));
            }
        }

        //No "Returns" subsection for indexers (there is a "Value" section instead)
        protected override void AddReturnsSubSection(MdContainerBlock block, IndexerOverloadDocumentation overload, int headingLevel)
        { }
    }
}
