using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using NHibernateMinimalApiSample.Helpers;
using NHibernateMinimalApiSample.Models;
using ISession = NHibernate.ISession;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<NHibernateHelper>(provider =>
    new NHibernateHelper(builder.Configuration.GetConnectionString("DbConnection")!));

builder.Services.AddScoped(provider =>
    provider.GetRequiredService<NHibernateHelper>().OpenSession());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/products", async ([FromServices] ISession session) =>
{
    using var transaction = session.BeginTransaction();
    var products = await session.Query<Product>().ToListAsync();
    await transaction.CommitAsync();
    return products;
});

app.MapGet("/products/{id:int}", async (int id, [FromServices] ISession session) =>
{
    using var transaction = session.BeginTransaction();
    var product = await session.GetAsync<Product>(id);
    await transaction.CommitAsync();
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (Product product, [FromServices] ISession session) =>
{
    using var transaction = session.BeginTransaction();
    await session.SaveAsync(product);
    await transaction.CommitAsync();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:int}", async (int id, Product updatedProduct, [FromServices] ISession session) =>
{
    using var transaction = session.BeginTransaction();
    var existingProduct = await session.GetAsync<Product>(id);
    if (existingProduct is null) return Results.NotFound();

    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;

    await session.UpdateAsync(existingProduct);
    await transaction.CommitAsync();
    return Results.Ok(existingProduct);
});

app.MapDelete("/products/{id:int}", async (int id, [FromServices] ISession session) =>
{
    using var transaction = session.BeginTransaction();
    var product = await session.GetAsync<Product>(id);
    if (product is null) return Results.NotFound();

    await session.DeleteAsync(product);
    await transaction.CommitAsync();
    return Results.NoContent();
});

app.Run();