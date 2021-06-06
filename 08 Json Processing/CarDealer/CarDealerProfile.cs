using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using CarDealer.Models;
using CarDealer.DTO;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            //Supplier
            CreateMap<ImportSupplierDto, Supplier>();


            //Part
            CreateMap<ImportPartDto, Part>();


            //Car
           

            //Customer
            CreateMap<ImportCustomerDto, Customer>();

            //Sale
            CreateMap<ImportSaleDto, Sale>();

        }
    }
}
