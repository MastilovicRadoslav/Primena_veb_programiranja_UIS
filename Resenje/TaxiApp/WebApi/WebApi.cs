using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;
using System.Text;

namespace WebApi
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class WebApi : StatelessService
    {
        public WebApi(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        //jwt
                        var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>(); //izdavac tokena
                        var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>(); //tajni kljuc za potpisivanje tokena
                        //Registracija IEmailService kao tranzijenti (nova instanca EmailSenderModel se kreira svaki put kad se zatrazi ovaj servis)
                        builder.Services.AddTransient<IEmailService,EmailSenderModel>();
                        //konfiguraicja JWT autentifikacije
                        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //šema
                         .AddJwtBearer(options => //konfiguracija param. za validaciju tokena
                         {
                             options.TokenValidationParameters = new TokenValidationParameters //validacija tokena
                             {
                                 ValidateIssuer = true, //izdavac
                                 ValidateAudience = true, //publika
                                 ValidateLifetime = true, //da li je istekao
                                 ValidateIssuerSigningKey = true, //potpis
                                 ValidIssuer = jwtIssuer,
                                 ValidAudience = jwtIssuer,
                                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                             };
                         });
                        //jwt


                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext); //singelton servis ista instanca
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
                        builder.Services.AddControllers(); //registracija kontrolera
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();
                        builder.Services.AddSignalR(); //registracija SingalR biblioteke
                        //politika autorizacije za razlicite uloge korisnika
                        builder.Services.AddAuthorization(options =>
                        {
                               options.AddPolicy("Admin", policy => policy.RequireClaim("MyCustomClaim", "Admin"));
                               options.AddPolicy("Rider", policy => policy.RequireClaim("MyCustomClaim", "Rider"));
                               options.AddPolicy("Driver", policy => policy.RequireClaim("MyCustomClaim", "Driver"));
                        });

                          builder.Services.AddCors(options =>
                        {
                            options.AddPolicy(name: "cors", builder => {
                                builder.WithOrigins("http://localhost:3000") //dozvola zahtevima samo sa ovog URL
                                        .AllowAnyHeader() //bilo koji http
                                        .AllowAnyMethod() //bilo koja http metoda
                                        .AllowCredentials(); //slanje kredencijala

                                });
                            });



                        var app = builder.Build();
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }
                        app.UseCors("cors");
                        app.UseRouting();
                        app.UseHttpsRedirection();


                        app.UseAuthentication(); //aktivacija autentifikacije
                        app.UseAuthorization(); //aktivacija autorizacije

                        app.MapControllers(); //mapiranje ruta na kontrolere
                        app.UseStaticFiles();
                        app.UseFileServer();
                        app.UseDefaultFiles();


                        return app;

                    }))
            };
        }
    }
}
