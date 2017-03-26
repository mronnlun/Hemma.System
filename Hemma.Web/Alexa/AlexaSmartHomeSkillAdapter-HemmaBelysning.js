var https = require('https');

var studyCeilingApplianceId = "A146-3456-b31d-7ec4c146c5ea";
var entraceCeilingLightApplianceId = "A146-3456-b31d-7ec4c146c5eb";

var hemmaServer = "ronnlund.duckdns.org";
var hemmaPath = "/api/alexabelysning";

/**
 * Main entry point.
 * Incoming events from Alexa Lighting APIs are processed via this method.
 */
exports.handler = function (event, context) {

    log('Input', event);
    log('Input event data', JSON.stringify(event));

    switch (event.header.namespace) {

        /**
         * The namespace of "Discovery" indicates a request is being made to the lambda for
         * discovering all appliances associated with the customer's appliance cloud account.
         * can use the accessToken that is made available as part of the payload to determine
         * the customer.
         */
        case 'Alexa.ConnectedHome.Discovery':
            handleDiscovery(event, context);
            break;

        /**
         * The namespace of "Control" indicates a request is being made to us to turn a
         * given device on, off or brighten. This message comes with the "appliance"
         * parameter which indicates the appliance that needs to be acted on.
         */
        case 'Alexa.ConnectedHome.Control':
            handleControl(event, context);
            break;

        /**
         * We received an unexpected message
         */
        default:
            log('Err', 'No supported namespace: ' + event.header.namespace);
            context.fail('Something went wrong');
            break;
    }
};

/**
 * This method is invoked when we receive a "Discovery" message from Alexa Connected Home Skill.
 * We are expected to respond back with a list of appliances that we have discovered for a given
 * customer. 
 */
function handleDiscovery(event, context) {

    var accessToken = event.payload.accessToken;
    var message_id = event.header.messageId;

    log("Access Token: ", accessToken);

    var options = {
        hostname: hemmaServer,
        port: 443,
        path: hemmaPath + "/discovery",
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        }
    };

    log("options", JSON.stringify(options));

    var data = "accesstoken=" + accessToken

    log(data);

    var serverError = function (e) {
        log('Error', e.message);
        context.fail(generateControlError('DiscoveryRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
    };

    var callback = function (response) {
        var discoveredAppliances;

        response.on('data', function (chunk) {
            discoveredAppliances = JSON.parse(chunk);
        });

        response.on('end', function () {

            var headers = {
                namespace: 'Alexa.ConnectedHome.Discovery',
                name: 'DiscoverAppliancesResponse',
                payloadVersion: '2',
                messageId: message_id,
            };

            var result = {
                header: headers,
                payload:
                {
                    "discoveredAppliances": discoveredAppliances
                }
            };

            log("result", JSON.stringify(result));
            context.succeed(result);
        });

        response.on('error', serverError);
    };

    var req = https.request(options, callback);

    req.on('error', serverError);

    req.write(data);
    req.end();
}

/**
 * Control events are processed here.
 * This is called when Alexa requests an action (IE turn off appliance).
 */
function handleControl(event, context) {
    if (event.header.namespace === 'Alexa.ConnectedHome.Control') {

        /**
         * Retrieve the appliance id and accessToken from the incoming message.
         */
        var accessToken = event.payload.accessToken;
        var applianceId = event.payload.appliance.applianceId;
        var message_id = event.header.messageId;
        var param = "";
        var index = "0";
        var state = 0;
        var confirmation;
        var funcName;

        if (event.header.name == "TurnOnRequest") {
            state = 1;
            confirmation = "TurnOnConfirmation";
            funcName = "onoff";
        }
        else if (event.header.name == "TurnOffRequest") {
            state = 0;
            confirmation = "TurnOffConfirmation";
            funcName = "onoff";
        }
        else if (event.header.name == "SetPercentageRequest") {
            state = event.payload.percentageState.value;
            confirmation = "SetPercentageConfirmation";
            funcName = "setvalue";
        }

        log('applianceId', applianceId);

        var options = {
            hostname: hemmaServer,
            port: 443,
            path: hemmaPath + "/action?" + "function=" + funcName + "&applianceid=" + applianceId + "&state=" + state,
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        };

        log("options", JSON.stringify(options));

        var data = "accesstoken=" + accessToken;

        log(data);

        var serverError = function (e) {
            log('Error', e.message);
            context.fail(generateControlError('TurnOnRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
        };

        var callback = function (response) {
            var str = '';

            response.on('data', function (chunk) {
                str += chunk.toString('utf-8');
            });

            response.on('end', function () {
                log('Return Value');
                log(str);

                var headers = {
                    namespace: 'Alexa.ConnectedHome.Control',
                    name: confirmation,
                    payloadVersion: '2',
                    messageId: message_id
                };
                var payloads = {

                };
                var result = {
                    header: headers,
                    payload: payloads
                };

                context.succeed(result);
            });

            response.on('error', serverError);
        };

        var req = https.request(options, callback);

        req.on('error', serverError);

        req.write(data);
        req.end();
    }
}

/**
 * Utility functions.
 */
function log(title, msg) {
    console.log(title + ": " + msg);
}

function generateControlError(name, code, description) {
    var headers = {
        namespace: 'Control',
        name: name,
        payloadVersion: '1'
    };

    var payload = {
        exception: {
            code: code,
            description: description
        }
    };

    var result = {
        header: headers,
        payload: payload
    };

    return result;
}