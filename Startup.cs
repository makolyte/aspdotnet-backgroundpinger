using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace BackgroundPinger
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
            services.AddControllers();

            var seriFileLogger = new LoggerConfiguration().WriteTo.File(@"C:\Temp\Logs\log.txt").CreateLogger();
            services.AddSingleton<Serilog.ILogger>(seriFileLogger);

            services.AddSingleton<IPingSettings>(new PingSettings() 
            { 
                Timeout = TimeSpan.FromSeconds(30),
                Frequency = TimeSpan.FromSeconds(1),
                Target = "www.google.com"
            });


            services.AddHostedService<PingerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}