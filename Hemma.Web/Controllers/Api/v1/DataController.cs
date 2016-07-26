using Hemma.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Hemma.Web.Controllers.Api.v1
{
    public class DataController : ApiController
    {
        [ArrayInput("fields")]
        public IEnumerable<IEnumerable<KeyValuePair<string, object>>> Get(string databaseName, int millisecondOffset, string[] fields)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>("Data");

            var startTime = DateTime.Now.AddMilliseconds(millisecondOffset * -1).Ticks;
            var endTime = DateTime.Now.Ticks;

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Gte("_id", startTime) & builder.Lte("_id", endTime);

            ProjectionDefinition<BsonDocument> projection = null;

            var fieldsToInclude = fields != null ? fields : new string[0];

            foreach (string columnName in fieldsToInclude)
            {
                if (projection == null)
                {
                    projection = Builders<BsonDocument>.Projection.Include(columnName);
                }
                else
                {
                    projection = projection.Include(columnName);
                }
            }

            var find = collection.Find(filter);
            if (projection != null)
                find = find.Project(projection);

            var result = find.ToList();

            var selectedItems = new List<List<KeyValuePair<string, object>>>();
            foreach (var row in result)
            {
                var values = new List<KeyValuePair<string, object>>();
                foreach (var element in row.Elements)
                {
                    object itemValue = null;
                    if (element.Value.IsString)
                        itemValue = element.Value.AsString;
                    else if (element.Value.IsInt32)
                        itemValue = element.Value.AsInt32;
                    else if (element.Value.IsInt64)
                        itemValue = element.Value.AsInt64;
                    else if (element.Value.IsValidDateTime)
                        itemValue = element.Value.ToLocalTime();

                    var keyValue = new KeyValuePair<string, object>(element.Name, itemValue);
                    values.Add(keyValue);
                }
                selectedItems.Add(values);
            }

            return selectedItems;
        }


    }
}