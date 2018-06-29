﻿using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Address.Annotations;

namespace NCS.DSS.Address.Models
{
    public class AddressPatch
    {
        private const string AddressRegEx = @"[A-Za-z0-9 ~!@&amp;'\()*+,\-.\/:;]{1,100}";
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})";

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 1")]
        [Example(Description = "Adddress Line 1")]
        public string Address1 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 2")]
        [Example(Description = "Adddress Line 2")]
        public string Address2 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 3")]
        [Example(Description = "Adddress Line 3")]
        public string Address3 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 4")]
        [Example(Description = "Adddress Line 4")]
        public string Address4 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 5")]
        [Example(Description = "Adddress Line 5")]
        public string Address5 { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx, ErrorMessage = "Please enter a valid postcode")]
        [Display(Description = "Customers postcode within England.")]
        [Example(Description = "AA11AA")]
        public string PostCode { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx)]
        [Display(Description = "This should be used where the customers home address is not within England.")]
        [Example(Description = "AA11AA")]
        public string AlternativePostCode { get; set; }

        [RegularExpression(@"^(\+|-)?(?:180(?:(?:\.0{1,6})?)|(?:[0-9]|[1-9][0-9]|1[0-7][0-9])(?:(?:\.[0-9]{1,6})?))$")]
        [Display(Description = "Geocoded address information")]
        [Example(Description = "-1.50812")]
        public decimal Longitude { get; set; }

        [RegularExpression(@"^(\+|-)?(?:90(?:(?:\.0{1,6})?)|(?:[0-9]|[1-8][0-9])(?:(?:\.[0-9]{1,6})?))$")]
        [Display(Description = "Geocoded address information")]
        [Example(Description = "52.40100")]
        public decimal Latitude { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer started residing at this location")]
        [Example(Description = "2018-06-19T09:01:00")]
        public DateTime EffectiveFrom { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer end residence at this location")]
        [Example(Description = "2018-06-21T13:12:00")]
        public DateTime EffectiveTo { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-21T13:45:00")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}