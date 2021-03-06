﻿function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function addSeconds(time, seconds) {
    var milliseconds = seconds * 1000;
    var newTime = new Date();
    newTime.setTime(time.getTime() + milliseconds);
    return newTime;
}

function getTimeFromMinutes(minutes) {
    var startTime = addSeconds(new Date(), -1 * minutes * 60);
    return startTime;
}


Handlebars.registerHelper('dateFormat', function (context, block) {
    if (window.moment) {
        var f = block.hash.format || "D.M.YYYY H:mm";
        var date = moment.parseZone(context);
        var formattedDate = date.local().format(f);
        return formattedDate;
    } else {
        return context;   //  moment plugin not available. return data as is.
    };
});


