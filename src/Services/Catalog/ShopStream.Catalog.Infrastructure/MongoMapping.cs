using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Infrastructure;

/// <summary>
/// BSON mapping lives here, not as attributes on the domain types, so the Domain
/// project has no dependency on MongoDB.
/// </summary>
public static class MongoMapping
{
    private static readonly object Gate = new();

    public static void Register()
    {
        lock (Gate)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Product)))
            {
                BsonClassMap.RegisterClassMap<Product>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.MapIdMember(p => p.Id)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId))
                        .SetIdGenerator(StringObjectIdGenerator.Instance);
                    // The driver stores decimal as a string by default, which breaks range queries and sorting.
                    cm.MapMember(p => p.Price).SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Category)))
            {
                BsonClassMap.RegisterClassMap<Category>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.MapIdMember(c => c.Id)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId))
                        .SetIdGenerator(StringObjectIdGenerator.Instance);
                });
            }
        }
    }
}
