using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskBud.Website.Swagger
{
    public class ApiAuthenticationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Bearer",
                Description = "Bearer token, see [See here](/Identity/Account/Manage/ApiAccess)",
                Required = true,
                In = ParameterLocation.Query,
                AllowEmptyValue = false
            });
        }
	}
}
