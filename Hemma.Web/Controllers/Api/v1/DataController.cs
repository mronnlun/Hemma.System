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
            if (field.Equals("KompressorDrifttidVarme", StringComparison.CurrentCultureIgnoreCase))
            {
                projection = projection.Include("KompressorDifttid");
                projection = projection.Include("KompressorDrifttidVarmvatten");
            }
            else
                projection = projection.Include(field);

            var sort = Builders<BsonDocument>.Sort.Ascending("Timestamp");

            var find = collection.Find(filter).Project(projection).Sort(sort);

            var results = find.ToList();

            var valueItems = new List<List<object>>();
            foreach (var row in results)
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
                valueItems.Add(values);
            }

            if (field.Equals("KompressorDrifttidVarme", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (var valueItem in valueItems)
                {
                    var drifttid = (double)valueItem[1];
                    var drifttidVatten = (double)valueItem[2];
                    var drifttidVarme = drifttid - drifttidVatten;
                    valueItem.RemoveAt(2);
                    valueItem[1] = drifttidVarme;
                }
            }

            valueItems.RemoveAll(row =>
            {
                var value = row[1];
                if ((value is double && ((double)value).Equals(0)) || (value is int && (int)value == 0) ||
                (value is long && (long)value == 0))
                    return true;
                else
                    return false;
            });


            if (removeConsecutiveValues)
            {
                int i = 1;
                //From second item to next to last
                while (i < valueItems.Count - 1)
                {
                    var previous = valueItems[i - 1][1];
                    var current = valueItems[i][1];
                    var next = valueItems[i + 1][1];

                    if (previous.Equals(current) && current.Equals(next))
                        valueItems.RemoveAt(i);
                    else
                        i++;
                }
            }

            List<List<object>> deltaValues = null;

            if (field.Equals("Kompressorstarter", StringComparison.CurrentCultureIgnoreCase) ||
                field.Equals("KompressorDifttid", StringComparison.CurrentCultureIgnoreCase) ||
                field.Equals("KompressorDrifttidVarmvatten", StringComparison.CurrentCultureIgnoreCase) ||
                field.Equals("KompressorDrifttidVarme", StringComparison.CurrentCultureIgnoreCase))
            {
                var baseDate = new DateTime(1970, 1, 1);

                var serieslength = new DateTime(endTime) - new DateTime(startTime);
                var groupByHour = serieslength.TotalDays <= 4 ? true : false;

                var hourgroups = valueItems.GroupBy(item =>
                {
                    var milliseconds = (long)item[0];
                    var time = baseDate.AddMilliseconds(milliseconds);
                    var date = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                    if (groupByHour)
                        date = date.AddHours(time.Hour);

                    return date;
                });

                var hourvalues = new List<KeyValuePair<DateTime, int>>();
                foreach (var hourgroup in hourgroups)
                {
                    if (hourgroup.ElementAt(0)[1] is int)
                    {
                        var maxvalue = hourgroup.Max(item => (int)item[1]);
                        hourvalues.Add(new KeyValuePair<DateTime, int>(hourgroup.Key, maxvalue));
                    }
                    else if (hourgroup.ElementAt(0)[1] is double)
                    {
                        var maxvalue = hourgroup.Max(item => (double)item[1]);
                        hourvalues.Add(new KeyValuePair<DateTime, int>(hourgroup.Key, (int)maxvalue));
                    }
                }

                deltaValues = new List<List<object>>();
                int i = 1;
                while (i < hourvalues.Count)
                {
                    var current = hourvalues[i];
                    var previous = hourvalues[i - 1];

                    if (current.Value == previous.Value)
                        hourvalues.RemoveAt(i);
                    else
                    {
                        var values = new List<object>();
                        var deltavalue = current.Value - previous.Value;
                        if (deltavalue < 50 && (current.Key - previous.Key).TotalDays < 3) //TODO
                        {
                            values.Add((long)current.Key.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
                            values.Add(deltavalue);
                            deltaValues.Add(values);
                        }
                        i++;
                    }
                }
            }

            if (deltaValues != null)
                valueItems = deltaValues;

            return valueItems;
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
                return (long)value.ToLocalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            //else if (value.IsObjectId)
            //    itemValue = value.AsObjectId.ToString();

            return null;
        }
    }
}