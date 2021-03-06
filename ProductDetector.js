var res = API.getScreenResolutionMaintainRatio();

var lastCheck = 0;
var checkInterval = 500;
var detectorEnabled = true;

var lookingAtEntity = null;
var lookingAtEntityPos = null;

function Vector3Lerp(start, end, fraction) {
    return new Vector3(
        (start.X + (end.X - start.X) * fraction),
        (start.Y + (end.Y - start.Y) * fraction),
        (start.Z + (end.Z - start.Z) * fraction)
    );
}

function getLookingAtEntity() {
    var startPoint = API.getGameplayCamPos();
    var aimPoint = API.screenToWorldMaintainRatio(new PointF(res.Width / 2, res.Height / 2));
    startPoint.Add(aimPoint);

    var endPoint = Vector3Lerp(startPoint, aimPoint, 1.1);
    var rayCast = API.createRaycast(startPoint, endPoint, (1 | 16), null);
    if (!rayCast.didHitEntity) return null;

    var hitEntityHandle = rayCast.hitEntity;
    if (API.getEntityPosition(hitEntityHandle).DistanceTo(API.getEntityPosition(API.getLocalPlayer())) > 2) return null;
    if (!API.hasEntitySyncedData(hitEntityHandle, "Courier_DetectorID")) return null;
    return hitEntityHandle;
}

API.onKeyUp.connect(function(sender, e) {
    if (e.KeyCode == Keys.E)
    {
        if (API.isChatOpen() || API.isAnyMenuOpen() || lookingAtEntity == null || !detectorEnabled || !API.hasEntitySyncedData(lookingAtEntity, "Courier_DetectorID")) return;
        API.triggerServerEvent("Courier_Take", API.getEntitySyncedData(lookingAtEntity, "Courier_DetectorID"));
    }
});

API.onUpdate.connect(function() {
    if (detectorEnabled)
    {
        if (!API.isPlayerInAnyVehicle(API.getLocalPlayer()) && API.getGlobalTime() - lastCheck > checkInterval)
        {
            lookingAtEntity = getLookingAtEntity();
            if (lookingAtEntity != null) lookingAtEntityPos = API.getEntityPosition(lookingAtEntity);

            lastCheck = API.getGlobalTime();
        }

        if (!API.isAnyMenuOpen() && lookingAtEntity != null)
        {
            API.callNative("SET_DRAW_ORIGIN", lookingAtEntityPos.X, lookingAtEntityPos.Y, lookingAtEntityPos.Z + 0.75, 0);
            API.drawText("Press ~y~E ~w~to take.", 0, 0, 0.5, 255, 255, 255, 255, 4, 1, true, true, 500);
            API.callNative("CLEAR_DRAW_ORIGIN");
        }
    }
});