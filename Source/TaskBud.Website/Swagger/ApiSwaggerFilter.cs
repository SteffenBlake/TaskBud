using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskBud.Website.Swagger
{
    public class ApiSwaggerFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var nonApiPaths = swaggerDoc.Paths.Where(path => !path.Key.ToLower().StartsWith("/api/")).ToList();

            foreach (var nonApiPath in nonApiPaths)
            {
                swaggerDoc.Paths.Remove(nonApiPath.Key);
            }
        }
    }
}
