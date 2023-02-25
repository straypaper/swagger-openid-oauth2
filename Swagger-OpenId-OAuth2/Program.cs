using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
 
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
 
builder.Services.AddSwaggerGen(options =>
{
      var scheme = new OpenApiSecurityScheme()
      {
            Name = "Authorization",
            Description = "This API uses OAuth 2.0 with the authorization code flow.",
            Type = SecuritySchemeType.OAuth2,
            In = ParameterLocation.Header,
            Flows = new OpenApiOAuthFlows
            {
                  AuthorizationCode = new OpenApiOAuthFlow
                  {
                        AuthorizationUrl = new Uri("https://straypaper.eu.auth0.com/authorize"),
                        TokenUrl = new Uri("https://straypaper.eu.auth0.com/oauth/token"),
                        Scopes =
                        {
                              { "openid", "openid" },
                              { "profile", "profile" },
                              { "api", "api" }
                        }
                  }
            }
      };
 
      options.AddSecurityDefinition("OAuth", scheme);
 
      options.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
            {
                  new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                              Id = "OAuth",
                              Type = ReferenceType.SecurityScheme
                        }
                  },
                  new List<string>()
            }
      });
 
      options.SwaggerDoc("v1", new OpenApiInfo
      {
            Version = "v1",
            Title = "straypaper.com Example API",
            Description = "This is an example API that is called from Swagger, acting as an OAuth2 client application.",
            Contact = new OpenApiContact
            {
                  Name = "Sydney du Plooy",
                  Url = new Uri("https://www.straypaper.com/about")
            },
            License = new OpenApiLicense
            {
                  Name = "License",
                  Url = new Uri("https://github.com/straypaper/swagger-openid-oauth2/blob/main/LICENSE")
            }
      });
});
 
builder.Services.AddAuthentication(options =>
{
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
      options.Events = new()
      {
            OnTokenValidated = ctx => Task.Run(() => Debug.WriteLine($"Security Token: {ctx.SecurityToken}")),
            OnMessageReceived = ctx => Task.Run(() => Debug.WriteLine($"Message: {ctx.Token}")),
            OnForbidden = ctx => Task.Run(() => Debug.WriteLine($"Forbidden: {ctx}")),
            OnAuthenticationFailed = ctx => Task.Run(() => Debug.WriteLine($"Authentication failed: {ctx.Exception}")),
            OnChallenge = ctx => Task.Run(() => Debug.WriteLine($"Challenge error: {ctx.Error}"))
      };
      
      options.Authority = "https://straypaper.eu.auth0.com/";
      options.Audience = "https://localhost:7285";
      
      options.TokenValidationParameters = new TokenValidationParameters
      {
            LogValidationExceptions = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://straypaper.eu.auth0.com/",
      };
});
 
var app = builder.Build();
 
app.UseHttpLogging();
 
if (app.Environment.IsDevelopment())
{
      app.UseSwagger();
      app.UseSwaggerUI(c => {
            c.OAuthScopes("openid");
            c.EnablePersistAuthorization();
            c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> {
                  { "audience", "https://localhost:7285" }
            });
      });
}
 
app.UseHttpsRedirection();
 
app.UseAuthentication();
app.UseAuthorization();
 
app.MapControllers();
 
app.Run();
 