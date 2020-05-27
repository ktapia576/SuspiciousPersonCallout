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
        Ped suspect;
        Vector3 shackSandyShoresLocation = new Vector3(1573.99f, 3680.99f, 34.77f);
        Vector3 shackSandyShoresWalkToLocation = new Vector3(1574.93f, 3704.61f, 34.38f);

        Random rand = new Random();

        public SuspiciousPersonCallout()
        {
            InitBase(shackSandyShoresLocation);

            ShortName = "Suspicious Person";
            CalloutDescription = "Caller reported about a suspicious person walking around.";
            ResponseCode = 2;
            StartDistance = 100f;
        }
        public async override Task Init()
        {
            /* Called when the callout is accepted */

            /* Blip spawn happens in base.OnAccept() */
            this.OnAccept();

            /* Dispatch notifies player of situation */
            PrintNotification("~g~[Dispatch]: ~w~There are reports of someone trespassing into an abandoned building");

            /* Use the SpawnPed or SpawnVehicle method to get a properly networked ped (react to other players) */
            suspect = await SpawnPed(PedHash.FosRepCutscene, shackSandyShoresLocation, 32.82f);

            suspect.Weapons.Give(WeaponHash.Pistol, 100, false, true);

            suspect.AlwaysKeepTask = true;  // Have the Ped always keep task, no matter what happens around them
            suspect.BlockPermanentEvents = true;    // Prevent GTA V memory handling from deleting ped even when player is not near
        }
        public override void OnStart(Ped player)
        {
            /* Called when a player gets in range */

            base.OnStart(player); // -> to remove the blip from the map (yellow circle by default)

            suspect.AttachBlip();   // Attach a red player blip on map

            StartScene(suspect, shackSandyShoresWalkToLocation);
        }
        private async void StartScene(Ped ped, Vector3 location) 
        {
            await BaseScript.Delay(6000);   // Wait for player to reach close to location

            ped.Task.GoTo(location);    // Have Ped Walk to position on map with cords

            ped.Task.UseMobilePhone();
     
            await BaseScript.Delay(15000);   // Wait X milliseconds (5000msecs = 5secs)

            // Vector3 currentLocation = suspect.Position; // Get current ped position

            PrintSubtitle("Hey officer, why would you be here?", 2000);
            await BaseScript.Delay(2000);   // Wait 2 seconds and then run to give player time to read

            int number = rand.Next(101); // random integers between 0 and 100

            /* Randomize outcome */
            if (number >= 50)
            {
                Flee(ped);
            }
            else if (number < 50)
            {
                Attack(ped);
            }
            else 
            {
                Debug.WriteLine("Something went wrong with randomizing outcome.");  // Output to F8 Console
            }
        }
        private void Flee(Ped ped) 
        {
            ped.Task.FleeFrom(Game.PlayerPed);  // Have Ped flee from player
            Debug.WriteLine("Fleeing");  // Output to F8 Console
        }
        private void Attack(Ped ped) 
        {
            ped.Task.ShootAt(Game.PlayerPed);   // Have ped attack player
            Debug.WriteLine("Attacking");  // Output to F8 Console
        }
        private void PrintNotification(String message)
        {
            BeginTextCommandThefeedPost("STRING");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandThefeedPostTicker(false, true);
        }
        private void PrintSubtitle(String message, int duration)
        {
            BeginTextCommandPrint("STRING");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandPrint(duration, false);
        }
    }
}