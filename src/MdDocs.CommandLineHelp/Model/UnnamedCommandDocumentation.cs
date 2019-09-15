﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Mono.Cecil;

namespace Grynwald.MdDocs.CommandLineHelp.Model
{
    public sealed class UnnamedCommandDocumentation : CommandDocumentationBase
    {

        public UnnamedCommandDocumentation(
            ApplicationDocumentationBase application,
            IEnumerable<OptionDocumentation> options = null,
            IEnumerable<ValueDocumentation> values = null) : base(application, options, values)
        { }


        public static UnnamedCommandDocumentation FromTypeDefinition(ApplicationDocumentationBase application, TypeDefinition definition, ILogger logger)
        {
            if (application is null)
                throw new ArgumentNullException(nameof(application));

            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            // unnamed commands must not have a "verb" attribute
            if(definition.HasAttribute(Constants.VerbAttributeFullName))
                throw new ArgumentException("Cannot create unnamed command from type definition annotated with a Verb attribute", nameof(definition));

            return new UnnamedCommandDocumentation(application, LoadOptions(definition, logger), LoadValues(definition, logger));
        }
    }
}
