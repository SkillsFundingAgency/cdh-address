﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Address.Cosmos.Client;
using NCS.DSS.Address.Cosmos.Helper;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateCustomerDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<ResourceResponse<Document>> GetAddressAsync(Guid addressId)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(addressId);

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReadDocumentAsync(documentUri);

            return response;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var addressForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Address>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.AddressId == addressId)
                .AsDocumentQuery();

            if (addressForCustomerQuery == null)
                return null;

            var addressess = await addressForCustomerQuery.ExecuteNextAsync<Models.Address>();

            return addressess?.FirstOrDefault();
        }


        public async Task<List<Models.Address>> GetAddressesForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var queryAddresses = client.CreateDocumentQuery<Models.Address>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var addresses = new List<Models.Address>();

            while (queryAddresses.HasMoreResults)
            {
                var response = await queryAddresses.ExecuteNextAsync<Models.Address>();
                addresses.AddRange(response);
            }

            return addresses.Any() ? addresses : null;

        }

        public async Task<ResourceResponse<Document>> CreateAddressAsync(Models.Address address)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, address);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateAddressAsync(Models.Address address)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(address.AddressId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, address);

            return response;
        }
    }
}