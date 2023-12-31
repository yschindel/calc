﻿using System;
using System.Threading.Tasks;
using Speckle.Newtonsoft.Json;
using GraphQL;

namespace Calc.Core.DirectusAPI.Drivers
{
    public class DirectusManager<T> where T : class // e.g. T = ForestStorageDriver
    {
        private readonly Directus _directus;

        public DirectusManager(Directus directus)
        {
            this._directus = directus;
        }

        public async Task<TDriver> GetMany<TDriver>(IDriverGetMany<T> driver) where TDriver : IDriverGetMany<T>
        {
            var request = CreateRequest(driver.QueryGetMany, driver.GetVariables());
            var response = await this._directus.Client.SendQueryAsync<TDriver>(request);
            CheckResponseForErrors(response);
            return response.Data;
        }

        public async Task<TDriver> CreateSingle<TDriver>(IDriverCreateSingle<T> driver) where TDriver : IDriverCreateSingle<T>
        {
            CheckDriverPropertyIsNotNull(driver, "SendItem");
            var request = CreateRequest(driver.QueryCreateSingle, driver.GetVariables());
            var response = await this._directus.Client.SendMutationAsync<TDriver>(request);
            CheckResponseForErrors(response);
            return response.Data;
        }

        public async Task<TDriver> CreateMany<TDriver>(IDriverCreateMany<T> driver) where TDriver : IDriverCreateMany<T>
        {
            CheckDriverPropertyIsNotNull(driver, "SendItems");
            var request = CreateRequest(driver.QueryCreateMany, driver.GetVariables());
            var response = await this._directus.Client.SendMutationAsync<TDriver>(request);
            CheckResponseForErrors(response);
            return response.Data;
        }

        public async Task<TDriver> UpdateSingle<TDriver>(IDriverUpdateSingle<T> driver) where TDriver : IDriverUpdateSingle<T>
        {
            CheckDriverPropertyIsNotNull(driver, "SendItem");
            var request = CreateRequest(driver.QueryUpdateSingle, driver.GetVariables());
            var response = await this._directus.Client.SendMutationAsync<TDriver>(request);
            CheckResponseForErrors(response);
            return response.Data;
        }

        private GraphQLRequest CreateRequest(string query, object variables)
        {
            return new GraphQLRequest { Query = query, Variables = variables };
        }

        private void CheckDriverPropertyIsNotNull<TDriver>(TDriver driver, string propertyName)
        {
            if (driver == null)
            {
                throw new Exception($"Driver {typeof(TDriver).Name} is null.");
            }
            var property = typeof(TDriver).GetProperty(propertyName) ?? throw new Exception($"Property {propertyName} does not exist on driver {typeof(TDriver).Name}.");
            _ = property.GetValue(driver) ?? throw new Exception($"Property {propertyName} on driver {typeof(TDriver).Name} is null.");
        }

        private void CheckResponseForErrors<TReturnedDriver>(GraphQLResponse<TReturnedDriver> response)
        {
            if (response.Errors != null)
            {
                throw new Exception(JsonConvert.SerializeObject(response.Errors, Formatting.Indented));
            }
            //Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        }
    }
}
