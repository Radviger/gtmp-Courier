var isCarrying = false;
var carryingModel = -1;
var inMarkerType = 0; // 0- none | 1- factory | 2- buyer
var controlsToDisable = [12, 13, 14, 15, 16, 17, 24, 25, 37, 44, 45, 47, 58, 69, 70, 92, 114, 140, 141, 142, 143, 257, 263, 264, 331];

var courierVehicles = [-1743316013, 1162065741, -810318068, 65402552];
var closestVehicle = null;
var vehicleMenu = null;

function getClosestJobVehicle(maxDistance = 2.0) {
    var playerPos = API.getEntityPosition(API.getLocalPlayer());
    var tempMinDist = 9999.0;
    var vehicles = API.getStreamedVehicles();
    var tempVehicle = null;

    for (var i = 0; i < vehicles.Length; i++)
    {
        if (courierVehicles.indexOf(API.getEntityModel(vehicles[i])) == -1) continue;

        let tempDist = playerPos.DistanceTo(API.getEntityBoneWorldPosition(vehicles[i], "platelight"));
        if (tempDist > maxDistance) continue;

        if (tempDist < tempMinDist)
        {
            tempMinDist = tempDist;
            tempVehicle = vehicles[i];
        }
    }

    return tempVehicle;
}

API.onServerEventTrigger.connect(function(eventName, args) {
    switch (eventName)
    {
        case "Courier_SetCarryingState":
            isCarrying = args[0];
            carryingModel = args[1];

            // reset product detector
            resource.ProductDetector.detectorEnabled = !isCarrying;
            if (isCarrying) resource.ProductDetector.lookingAtEntity = null;

            API.disableVehicleEnteringKeys(isCarrying);
        break;

        case "Courier_SetMarkerType":
            inMarkerType = args[0];
        break;

        case "Courier_ReceiveVehicleProducts":
            if (vehicleMenu == null)
            {
                vehicleMenu = API.createMenu("Product Menu", " ", 0, 0, 6);

                vehicleMenu.OnItemSelect.connect(function(menu, item, index) {
                    API.triggerServerEvent(((isCarrying) ? "Courier_LoadToVehicle" : "Courier_TakeFromVehicle"), closestVehicle, index);
                    vehicleMenu.Visible = false;
                });
            }

            vehicleMenu.Clear();

            var data = JSON.parse(args[0]);
            for (var i = 0; i < data.length; i++) vehicleMenu.AddItem(API.createMenuItem(((data[i] == null) ? "Empty" : data[i].Name), ""));
            vehicleMenu.Visible = true;
        break;
    }
});

API.onEntityStreamIn.connect(function(entity, entityType) {
    if (API.getEntityType(entity) == 2 && API.hasEntitySyncedData(entity, "Courier_DetectorID")) API.callNative("SET_ENTITY_PROOFS", entity, true, true, true, true, true, true, 1, true);
});

API.onUpdate.connect(function() {
    if (isCarrying) for (var i = 0; i < controlsToDisable.length; i++) API.disableControlThisFrame(controlsToDisable[i]);
    if (inMarkerType > 0 && !API.isPlayerInAnyVehicle(API.getLocalPlayer())) API.displaySubtitle("Press ~y~E ~w~to interact with the " + ((inMarkerType == 1) ? "~b~Factory" : "~g~Buyer") + ".", 100);
});

API.onKeyUp.connect(function(e, key) {
    if (key.KeyCode == Keys.E)
    {
        if (API.isChatOpen() || resource.ProductDetector.lookingAtEntity != null || API.isPlayerInAnyVehicle(API.getLocalPlayer()) || API.isAnyMenuOpen()) return;

        switch (inMarkerType)
        {
            case 0: // player is not in any marker
                closestVehicle = getClosestJobVehicle();
                if (closestVehicle == null) return;

                API.triggerServerEvent("Courier_RequestVehicleProducts", closestVehicle);
            break;

            case 1: // player is in a factory marker
                API.triggerServerEvent(((isCarrying) ? "Courier_FactoryRefund" : "Courier_FactoryBuy"));
            break;

            case 2: // player is in a buyer marker
                if (isCarrying) API.triggerServerEvent("Courier_BuyerSell");
            break;
        }
    }

    if (key.KeyCode == Keys.X)
    {
        if (API.isChatOpen() || API.isAnyMenuOpen() || !isCarrying) return;
        var position = API.getOffsetInWorldCoords(API.getLocalPlayer(), new Vector3(0.0, 1.0, 0.0));

        // hackiest way 2018
        var invisiblePropToGetGroundPos = API.createObject(carryingModel, position, new Vector3());
        API.setEntityTransparency(invisiblePropToGetGroundPos, 0);
        API.callNative("PLACE_OBJECT_ON_GROUND_PROPERLY", invisiblePropToGetGroundPos);

        API.triggerServerEvent("Courier_Drop", API.getEntityPosition(invisiblePropToGetGroundPos));
        API.deleteEntity(invisiblePropToGetGroundPos);
    }
});

API.onResourceStop.connect(function() {
    API.disableVehicleEnteringKeys(false);
});