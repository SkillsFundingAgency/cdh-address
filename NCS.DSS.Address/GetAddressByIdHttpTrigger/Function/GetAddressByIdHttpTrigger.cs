using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using NCS.DSS.Address.Annotations;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.Ioc;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Function
{
    public static class GetAddressByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.Address))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single address with a given AddressId for an individual customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequestMessage req, TraceWriter log, string customerId, string addressId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetAddressByIdHttpTriggerService getAddressByIdService)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid) ||
                !Guid.TryParse(addressId, out var addressGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(addressId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent("Unable to find a customer with Id of : " +
                                                JsonConvert.SerializeObject(customerGuid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var address = await getAddressByIdService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (address == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent("Unable to find address with Id of : " + 
                                                JsonConvert.SerializeObject(addressId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(address),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}