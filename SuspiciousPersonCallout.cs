using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CalloutAPI;
using System.Runtime.InteropServices;

namespace SuspiciousPersonCallout
{
    [GuidAttribute("24b727e4-697a-462c-b7ab-d38c6e4c5b67")]
    [CalloutProperties("SuspiciousPerson", "KTaps", "1.0.0", Probability.Medium)]
    public class SuspiciousPersonCallout : CalloutAPI.Callout
    {
        Ped Suspect;

        public SuspiciousPersonCallout()
        {
            //Random rnd = new Random();

            Vector3 spawnLocation = new Vector3(1585.8f, 3646.41f, 34.52f);

            InitBase(spawnLocation);


            ShortName = "Suspicious Person";
            CalloutDescription = "Caller reported about a suspicious person walking around.";
            ResponseCode = 2;
            StartDistance = 40f;
        }
        public async override Task Init()
        {
            /* Called when the callout is accepted */
            /* Blip spawn happens in base.OnAccept() */
            this.OnAccept();

            /* Use the SpawnPed or SpawnVehicle method to get a properly networked ped (react to other players) */
            Suspect = await SpawnPed(PedHash.FosRepCutscene, this.Location, 210);

            Suspect.Weapons.Give(WeaponHash.Pistol, 100, false, true);

            Suspect.AlwaysKeepTask = true;  // Have the Ped always keep task, no matter what happens around them
            Suspect.BlockPermanentEvents = true;    // Prevent GTA V memory handling from deleting ped even when player is not near
        }
        public override void OnStart(Ped player)
        {
            /* Called when a player gets in range */

            base.OnStart(player); // -> to remove the blip from the map (yellow circle by default)

            Suspect.AttachBlip();   // Attach a red player blip on map
    
            // person.Task.FleeFrom(player); // Have Ped flee when player enters start position

            Suspect.Task.GoTo(new Vector3(1613.26f, 3593.69f, 35.15f));    // Have Ped Walk to position on map with cords

            Vector3 currentLocation = Suspect.Position; // Get current ped position

            Debug.WriteLine($"{currentLocation}");  // Output to Console

            if (Suspect.IsInRangeOf(currentLocation, 100f))
            {
                Notify("~y~[Callout]: ~w~The suspect is near the go to");
                Notify(currentLocation.ToString());
            }
            else
            {
                Notify("~y~[Callout]: ~w~The suspect is NOT near the go to");
                Notify(currentLocation.ToString());
            }
        }
        public void Notify(String message)
        {
            CitizenFX.Core.Native.API.BeginTextCommandThefeedPost("STRING");
            CitizenFX.Core.Native.API.AddTextComponentSubstringPlayerName(message);
            CitizenFX.Core.Native.API.EndTextCommandThefeedPostTicker(false, true);
        }
    }
}