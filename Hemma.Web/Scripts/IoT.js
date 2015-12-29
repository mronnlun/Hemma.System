
var currentTimeZoneOffset = new Date().getTimezoneOffset();

// converts date format from JSON
function getChartDate(d) {
    // offset in minutes is converted to milliseconds and subtracted so that chart's x-axis is correct
    return Date.parse(d) - (currentTimeZoneOffset * 60000);
}


function getAverage(values, index, fieldName, zone, currentValue) {

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
}

function valueSameAsNeighbours(values, index, fieldName) {

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
}

function getChartData(starttime, dataInterval, channelId, apiReadKey, successAction, failureAction) {
    getChartDataInternal(starttime, dataInterval, channelId, apiReadKey, successAction, failureAction, null, true);
}

function getChartDataInternal(starttime, dataInterval, channelId, apiReadKey, successAction, failureAction, previousData) {

    var dataUrl = "https://api.thingspeak.com/channels/" + channelId + "/feeds.json?api_key=" + apiReadKey;

    var endTime = addSeconds(new Date(), 10 * 60);

    if (starttime) {
        dataUrl = dataUrl + "&start=" + getTimeString(starttime);

        if (dataInterval > 0) {
            endTime = addSeconds(starttime, 8000 * dataInterval * 60);
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
            getChartDataInternal(addSeconds(endTime, 1), dataInterval, channelId, apiReadKey, successAction, failureAction, previousData, false);
    })
    .error(function () {
        if (failureAction)
            failureAction();
    });

}



function getFormattedData(data, fields) {

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

        var pointDate = getChartDate(dataPoint.created_at)

        for (var fieldNumber = 1; fieldNumber <= fields; fieldNumber++) {

            if (!valueSameAsNeighbours(data.feeds, index, "field" + fieldNumber)) {
                var value = parseFloat(dataPoint["field" + fieldNumber]);
                if (!isNaN(value)) {
                    var avg = getAverage(data.feeds, index, "field" + fieldNumber, 2, value);
                    if (Math.abs(value - avg) < 30)
                        formattedData["field" + fieldNumber].push([pointDate, value]);
                }
            }
        }
    }

    return formattedData;
}
