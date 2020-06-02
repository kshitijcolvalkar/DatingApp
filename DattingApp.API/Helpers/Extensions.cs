using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DattingApp.API.Helpers
{
    public static class Extensions
    {
        public static void WriteApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static void WritePaginationHeader(this HttpResponse response, int pageNumber, int pageSize, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(pageNumber, pageSize, totalItems, totalPages);

            var jsonFormatter = new JsonSerializerSettings();
            jsonFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver(); 

            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, jsonFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static int CalculateAge(this DateTime datetime)
        {
            var age = DateTime.Now.Year - datetime.Year;

            if(datetime.AddYears(age) > DateTime.Now)
                age--;
            
            return age;
        }
    }
}