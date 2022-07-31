using GraphQL.DynamicCRUD.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType(descriptor =>
    {
        // Loop all DBSet
        var dbSets = typeof(AppDbContext)
            .GetRuntimeFields();

        foreach (var dbSet in dbSets)
        {
            var name = dbSet.Name.Split('>')[0].Replace("<", "");
            var type = dbSet.FieldType.GetGenericArguments()[0];

            descriptor
                .Field($"get{name}")
                .Type(typeof(IQueryable<>).MakeGenericType(type))
                //.UsePaging(
                //    entityType: type,
                //    options: new PagingOptions
                //    {
                //        IncludeTotalCount = true
                //    })
                //.UseProjection()
                //.UseFiltering()
                //.UseSorting()
                .Resolve(r =>
                    {
                        var resolver = dbSet.GetValue(r.Service<AppDbContext>());

                        return resolver;
                    });
        }
    })
    .AddFiltering()
    .AddSorting()
    .AddProjections();

var app = builder.Build();

app.MapGraphQL();

app.Run();
