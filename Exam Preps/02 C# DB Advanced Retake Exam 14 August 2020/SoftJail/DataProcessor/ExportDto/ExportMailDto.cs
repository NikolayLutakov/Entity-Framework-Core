﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SoftJail.DataProcessor.ExportDto
{
    using System.Xml.Serialization;

    [XmlType("Message")]
    public class ExportMailDto
    {
        [XmlElement("Description")]
        public string Description { get; set; }

    }
}
