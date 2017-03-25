
var thingSpeak = (function () {

    function getTimeString(time) {
        var timeString = time.getUTCFullYear().toString() + "-" + zerofill(time.getUTCMonth() + 1, 2) + "-"
            + zerofill(time.getUTCDate(), 2) + "%20" + zerofill(time.getUTCHours(), 2)
            + ":" + zerofill(time.getUTCMinutes(), 2) + ":00"

        return timeString;
    }

    function zerofill(number, length) {
        // Setup
        var result = number.toString();
        var pad = length - result.length;

        while (pad > 0) {
            result = '0' + result;
            pad--;
        }

        return result;
    }

    var currentTimeZoneOffset = new Date().getTimezoneOffset();

    var config = {
        dataInterval: 3,
        channelId: null,
        apiReadKey: null
    };

    function getDataInternal(starttime, successAction, failureAction, previousData) {

        var dataUrl = "https://api.thingspeak.com/channels/" + config.channelId + "/feeds.json?api_key=" + config.apiReadKey;

        var endTime = addSeconds(new Date(), 10 * 60);

        if (starttime) {
            dataUrl = dataUrl + "&start=" + getTimeString(starttime);

            if (config.dataInterval > 0) {
                endTime = addSeconds(starttime, 8000 * config.dataInterval * 60);
                dataUrl = dataUrl + "&end=" + getTimeString(endTime);
            }
        }

        $.getJSON(dataUrl, function (data) {

            if (previousData == null)
                previousData = data;
            else if (data.feeds.length > 0)
                previousData.feeds = previousData.feeds.concat(data.feeds);

            if (endTime > new Date())
                successAction(previousData);
            else
                getDataInternal(addSeconds(endTime, 1), successAction, failureAction, previousData, false);
        })
        .error(function () {
            if (failureAction)
                failureAction();
        });

    };

    return {

        config: config,

        getLatestData: function (successAction) {
            $.getJSON("https://api.thingspeak.com/channels/" + this.config.channelId +
                "/feeds/last.json?api_key=" + this.config.apiReadKey, function (data) {
                    if (successAction)
                        successAction(data);
                });
        },

        getChartDate: function (d) {
            // offset in minutes is converted to milliseconds and subtracted so that chart's x-axis is correct
            return Date.parse(d) - (currentTimeZoneOffset * 60000);
        },


        getAverage: function (values, index, fieldName, zone, currentValue) {

            var surroundingvalues = [];
            for (var i = index - zone; i <= index + zone; i++) {
                if (i >= 0 && i < values.length && i != index) {
                    var value = 0;
                    value = parseInt(values[i][fieldName]);

                    if (!isNaN(value)) {
                        surroundingvalues.push(value);
                    }
                }
            }

            surroundingvalues.sort();
            //Remove first/smallest value
            surroundingvalues.shift();
            //Remove last/largest value
            surroundingvalues.pop();

            if (surroundingvalues.length == 0)
                return currentValue;

            var sum = 0;
            var count = 0;
            for (var i = 0; i < surroundingvalues.length; i++) {
                sum += surroundingvalues[i];
                count++;
            }

            var avg = sum / count;
            return avg;
        },

        valueSameAsNeighbours: function (values, index, fieldName) {

            //Check if current value is the same as previous and next
            if (index > 0 && index < values.length - 1) {
                var currentValue = values[index][fieldName];
                var prevValue = values[index - 1][fieldName];
                if (prevValue == currentValue) {
                    var nextValue = values[index + 1][fieldName];
                    if (nextValue == currentValue) {
                        return true;
                    }
                }
            }
            return false;
        },

        getData: function (starttime, successAction, failureAction) {
            getDataInternal(starttime, successAction, failureAction, null);
        },


        getFormattedData: function (data, fields, filterByAverage) {

            var formattedData = {
                field1: [],
                field2: [],
                field3: [],
                field4: [],
                field5: [],
                field6: [],
                field7: [],
                field8: [],
            };


            for (var index = 0; index < data.feeds.length; index++) {
                var dataPoint = data.feeds[index];

                var pointDate = this.getChartDate(dataPoint.created_at)

                for (var fieldNumber = 1; fieldNumber <= fields; fieldNumber++) {

                    var fieldName = "field" + fieldNumber;

                    if (!this.valueSameAsNeighbours(data.feeds, index, fieldName)) {
                        var value = parseFloat(dataPoint[fieldName]);
                        if (!isNaN(value)) {
                            if (filterByAverage) {
                                var avg = this.getAverage(data.feeds, index, fieldName, 2, value);
                                if (Math.abs(value - avg) < 30)
                                    formattedData[fieldName].push([pointDate, value]);
                            }
                            else
                                formattedData[fieldName].push([pointDate, value]);
                        }
                    }
                }
            }

            return formattedData;
        }
    }
})();