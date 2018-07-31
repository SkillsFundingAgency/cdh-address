using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Annotations;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Ioc;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public static class PostAddressHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Address))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")]HttpRequestMessage req, ILogger log, string customerId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPostAddressHttpTriggerService addressPostService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (touchpointId == null)
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Post Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            Models.Address addressRequest;

            try
            {
                addressRequest = await httpRequestMessageHelper.GetAddressFromRequest<Models.Address>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (addressRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            addressRequest.LastModifiedTouchpointId = touchpointId;

            var errors = validate.ValidateResource(addressRequest, true);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var address = await addressPostService.CreateAsync(addressRequest);

            return address == null
                ? HttpResponseMessageHelper.BadRequest(customerGuid)
                : HttpResponseMessageHelper.Created(address);
        }
    }
}