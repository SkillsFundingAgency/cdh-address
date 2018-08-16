﻿using System;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public interface IPatchAddressHttpTriggerService
    {
        Task<Models.Address> UpdateAsync(Models.Address address, Models.AddressPatch addressPatch);
        Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
        Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl);
    }
}