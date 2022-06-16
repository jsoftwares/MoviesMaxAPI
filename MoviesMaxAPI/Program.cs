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
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

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

//ensures d user email claim is not in ClaimsType.email(URL type, which is the default) format but an email
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

/**Service for IdentityCore. We pass 2 params: a data type that represents a user in our system & a data type that reps a role(if we want
*to use role. We also added a config for authentication; d idea is, there are diff ways to configure this, you could use cookies, but we
*are going to use Json Web Token in this project. We pass a fn as argument of AddJwtBearer() to configure d paramters for validating 
*Json web token as we only want to accept valid tokens
**/
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,    //we're going to use this bcos we want to ensure d token is invalid when it's passed its expiration time
            ValidateIssuerSigningKey = true, //means we're going to validate that d JWT is valid through its signing key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtkey"])),   //we can also use builder.Configuration.GetValue<string>("keyjwt") here like we used above. Diff variation of getting a value from our appsettings.json file
            ClockSkew = TimeSpan.Zero
        };
    });

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
