using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dtos.Import
{
    [XmlType("Sale")]
    public class ImportSaleDto
    {
        [XmlElement("carId")]
        public int CarId { get; set; }
        [XmlElement("customerId")]
        public int CustomerId { get; set; }
        [XmlElement("discount")]
        public int Discount { get; set; }
    }
}

/*
<Sales>
    <Sale>
        <carId>105</carId>
        <customerId>30</customerId>
        <discount>30</discount>
    </Sale>
    <Sale>
*/