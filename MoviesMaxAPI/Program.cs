using Microsoft.AspNetCore.Authentication.JwtBearer;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MoviesMaxAPI;
using MoviesMaxAPI.APIBehaviour;
using MoviesMaxAPI.Filters;
using MoviesMaxAPI.Helpers;
using NetTopologySuite.Geometries;
using NetTopologySuite;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

//builder.Services.AddControllers();
builder.Services.AddControllers(
    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MoviesMaxAPI", Version = "v1" });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

/** we added a callback function as d 2nd parameter of AddDbContext() to add NetTopologySuite to EntityFrameworkCore which allow us use/store Point data type 
 * we also inject as a service a class from NetTopologySuite that allows us to work with distances and transformations  for NetTopologySuite.
 * srid of 4326 in other to work with distances on planet earth
 * **/
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.UseNetTopologySuite()));
builder.Services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

//we add configuration that allows us d GeometryFactory into our AutoMapper configuration class using dependency injection
builder.Services.AddSingleton(provider => new MapperConfiguration(config =>
{
   var geometryFactory = provider.GetRequiredService<GeometryFactory>();
   config.AddProfile(new AutoMapperProfiles(geometryFactory));
}).CreateMapper());

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ParseBadRequest));
}).ConfigureApiBehaviorOptions(BadRequestsBehavior.Parse);
builder.Services.AddCors( options =>
{
    var frontendUrl = builder.Configuration.GetValue<string>("frontend_url");
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders(new string[] { "totalAmountOfRecords" });
    });
});

builder.Services.AddScoped<IFileStorageService, AzureStorageService>();
//builder.Services.AddScoped<IFileStorageService, InAppStorageService>();
//builder.Services.AddHttpContextAccessor();      //used with storing files locally

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MoviesMaxAPI v1"));
}

app.UseHttpsRedirection();

//app.UseStaticFiles();       //needed for storing files locally

//app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
