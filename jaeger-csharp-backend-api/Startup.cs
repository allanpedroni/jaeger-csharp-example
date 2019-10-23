using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using System.Net.Http;

namespace jaeger_csharp_backend_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddOpenTracing();
            services.AddTransient<HttpClient>();

            services.AddLogging(configure =>
            {
                configure.ClearProviders();

                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration);
                    //.Enrich.WithExceptionDetails();

                //loggerConfiguration = loggerConfiguration
                //    .Enrich.FromLogContext()
                //    .Enrich.WithProcessId()
                //    .Enrich.WithProcessName()
                //    .Enrich.WithThreadId()
                //    .Enrich.WithEnvironmentUserName()
                //    .Enrich.WithMachineName();

                loggerConfiguration = loggerConfiguration.WriteTo
                    .Async(a => a.Console(new JsonFormatter()));

                Log.Logger = loggerConfiguration.CreateLogger();

                configure.AddSerilog();
                configure.AddDebug();
            });

            // Adds the Jaeger Tracer.
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                string serviceName = serviceProvider.GetRequiredService<IHostingEnvironment>().ApplicationName;

                var reporter = new LoggingReporter(loggerFactory);

                var sampler = new ConstSampler(true);

                // This will log to a default localhost installation of Jaeger.
                var tracer = new Tracer.Builder(serviceName)
                    //.WithLoggerFactory(loggerFactory)
                    .WithReporter(reporter)
                    .WithSampler(sampler)
                    .Build();

                // Allows code that can't use DI to also access the tracer.
                GlobalTracer.Register(tracer);

                return tracer;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
