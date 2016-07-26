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
        public IEnumerable<IEnumerable<KeyValuePair<string, object>>> GetFieldsValues(string databaseName, string[] fields, int secondOffset = 3600, bool includeKey = false)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>("LoggedData");

            var startTime = DateTime.Now.AddSeconds(secondOffset * -1).Ticks;
            var endTime = DateTime.Now.Ticks;

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Gte("Timestamp", startTime) & builder.Lte("Timestamp", endTime);

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

            var sort = Builders<BsonDocument>.Sort.Ascending("Timestamp");
            find = find.Sort(sort);

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
                    else if (element.Value.IsBoolean)
                        itemValue = element.Value.AsBoolean;
                    else if (element.Value.IsDouble)
                        itemValue = element.Value.AsDouble;
                    else if (element.Value.IsValidDateTime)
                        itemValue = element.Value.ToLocalTime();
                    else if (includeKey && element.Value.IsObjectId)
                        itemValue = element.Value.AsObjectId.ToString();

                    if (itemValue != null)
                    {
                        var keyValue = new KeyValuePair<string, object>(element.Name, itemValue);
                        values.Add(keyValue);
                    }
                }
                selectedItems.Add(values);
            }

            return selectedItems;
        }

        public IEnumerable<IEnumerable<object>> GetTimestampValues(string databaseName, string field, int secondOffset = 3600,
            bool removeConsecutiveValues = true)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                return null;

            if (string.IsNullOrWhiteSpace(field))
                return null;

            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>("LoggedData");

            var startTime = DateTime.Now.AddSeconds(secondOffset * -1).Ticks;
            var endTime = DateTime.Now.Ticks;

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Gte("Timestamp", startTime) & builder.Lte("Timestamp", endTime);

            var projection = Builders<BsonDocument>.Projection.Include("Datestamp");
            projection = projection.Include(field);

            var sort = Builders<BsonDocument>.Sort.Ascending("Timestamp");

            var find = collection.Find(filter).Project(projection).Sort(sort);

            var result = find.ToList();

            if (removeConsecutiveValues)
            {
                int i = 1;
                //From second item to next to last
                while (i < result.Count - 1)
                {
                    var previous = result[i - 1].GetElement(2).Value;
                    var current = result[i].GetElement(2).Value;
                    var next = result[i + 1].GetElement(2).Value;

                    if (previous.Equals(current) && current.Equals(next))
                        result.RemoveAt(i);
                    else
                    {
                        //if (current.IsNumeric)
                        //{
                        //    var previousNumber = previous.AsDouble;
                        //    var currentNumber = current.AsDouble;
                        //    var nextNumber = next.AsDouble;

                        //    if ()
                        //}
                        i++;
                    }
                }
            }

            var selectedItems = new List<List<object>>();
            foreach (var row in result)
            {
                var values = new List<object>();
                foreach (var element in row.Elements)
                {
                    object itemValue = GetValue(element.Value);

                    if (itemValue != null)
                    {
                        values.Add(itemValue);
                    }
                }
                selectedItems.Add(values);
            }

            return selectedItems;
        }

        private object GetValue(BsonValue value)
        {
            if (value.IsString)
                return value.AsString;
            else if (value.IsInt32)
                return value.AsInt32;
            else if (value.IsInt64)
                return value.AsInt64;
            else if (value.IsBoolean)
                return value.AsBoolean;
            else if (value.IsDouble)
                return value.AsDouble;
            else if (value.IsValidDateTime)
                return value.ToLocalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            //else if (value.IsObjectId)
            //    itemValue = value.AsObjectId.ToString();

            return null;
        }
    }
}