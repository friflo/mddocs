﻿using System;
using NuDoq;

namespace MdDoc.Model.XmlDocs
{
    public sealed class CElement : Element
    {
        private readonly C m_NuDoqModel;

        
        public string Content => m_NuDoqModel.Content;


        internal CElement(C nuDoqModel)
        {
            m_NuDoqModel = nuDoqModel ?? throw new ArgumentNullException(nameof(nuDoqModel));            
        }

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
