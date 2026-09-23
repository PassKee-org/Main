using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using PassKee.Business.Helpers;
using PassKee.Business.Mvc.Filters;
using PassKee.Business.Services.Http;

namespace PassKee.Business.Extensions
{
    public static class AppInitExtensions
    {
        public static void InitControllers(this IServiceCollection services, Assembly assembly)
        {
            services.AddControllers(options =>
                {
                    options.Filters.Add<ExceptionHandlerActionFilter>();
                })
                // We should provide correct assembly for the tests
                .AddApplicationPart(assembly)
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Disable pre-model validation of the models
                    options.SuppressModelStateInvalidFilter = true;
                    
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        // Get an instance of ILogger (see below) and log accordingly.
                        var body = context.HttpContext.Request.ReadBodyAsync().Result;
                        Log.Logger.Error($"Request data: {body}");
                        foreach (var value in context.ModelState.Values)
                        {
                            foreach (var error in value.Errors)
                            {
                                var errorMessage = !string.IsNullOrEmpty(error.ErrorMessage)
                                    ? error.ErrorMessage
                                    : error.Exception?.Message;
                                Log.Logger.Error(errorMessage);
                            }
                        }
                        return new BadRequestObjectResult(context.ModelState);
                    };
                })
                .AddNewtonsoftJson(options =>
                {
                    // Remove nullable fields from response Json
                    // options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    
                    // This is fix for the Headers. This resolver fix
                    // a bug when "authorization" header is not equals "Authorization" 
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    // options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddJsonOptions(options => {
                    // Ignore Null values in response models
                    // options.JsonSerializerOptions.IgnoreNullValues = true;
                });
        }

        public static void InitApiAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecurityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    configuration.GetValue<string>("App:Auth:SymmetricSecurityKey")!
                )
            );
            services.AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.IncludeErrorDetails = ApplicationHelper.HostingEnvironment != "Production";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = configuration.GetValue<string>("App:Auth:Issuer"),
                        ValidAudience = configuration.GetValue<string>("App:Auth:Audience"), 
                        IssuerSigningKey = jwtSecurityKey,
                        ClockSkew = System.TimeSpan.FromMinutes(5),
                        LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                        {
                            return notBefore <= DateTime.UtcNow && expires >= DateTime.UtcNow;
                        }
                    };
                    options.Events = new JwtBearerEvents { 
                        
                        OnMessageReceived = (context) => {
                            var tokenResolver = context.HttpContext.RequestServices.GetService<IHttpTokenResolverService>();
                            if (tokenResolver == null)
                            {
                                Log.Logger.Error($"IHttpTokenResolverService service can not be resolved");
                                return Task.CompletedTask;
                            }

                            var token = tokenResolver.GetApiToken();
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
