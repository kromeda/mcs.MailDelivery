using FluentValidation.AspNetCore;
using MailDelivery.Data;
using MailDelivery.Extenssions;
using MailDelivery.Middleware;
using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using MailDelivery.Services;
using MailDelivery.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace MailDelivery
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<MailDeliveryConfiguration>(Configuration.GetSection(nameof(MailDeliveryConfiguration)));
            services
                .AddControllers()
                .AddFluentValidation(options =>
                {
                    options.RegisterValidatorsFromAssemblyContaining<Startup>();
                    options.DisableDataAnnotationsValidation = true;
                });

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("SqliteConnection")));
            services.AddTransient<IDbRepository, SqliteRepository>();
            services.AddTransient<ILetterValidator, LetterValidator>();

            services.AddQuartzSendLetters();
            services.AddQuartzServer(options => options.WaitForJobsToComplete = true);

            services.AddHttpClients();
            services.AddMessageAdapters();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}