﻿using NuDoq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdDoc.Model.XmlDocs
{
    public sealed class ExampleElement : ContainerElement
    {        
        public ExampleElement(IEnumerable<Element> elements) : base(elements)
        { }


        public override TResult Accept<TResult, TParameter>(IVisitor<TResult, TParameter> visitor, TParameter parameter) => visitor.Visit(this, parameter);
    }
}
