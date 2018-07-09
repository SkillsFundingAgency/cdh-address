using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Address.Annotations;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Ioc;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Function
{
    public static class PatchAddressHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.AddressPatch))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing address.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequestMessage req, TraceWriter log, string customerId, string addressId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchAddressHttpTriggerService addressPatchService)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return HttpResponseMessageHelper.BadRequest(addressGuid);

            // Get request body
            var addressPatch = await httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(req);

            if (addressPatch == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);
           
            // validate the request
            var errors = validate.ValidateResource(addressPatch);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity("Validation error(s) : ", errors);
           
            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent("Unable to find a customer with Id of : ", customerGuid);
           
            var address = await addressPatchService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (address == null)
                return HttpResponseMessageHelper.NoContent("Unable to find a address with Id of : ", addressGuid);

           var updatedAddress = await addressPatchService.UpdateAsync(address, addressPatch);

            return updatedAddress == null ? 
                HttpResponseMessageHelper.BadRequest("Unable to find update address with Id of : ", addressGuid) :
                HttpResponseMessageHelper.Ok(updatedAddress);
        }
    }
}