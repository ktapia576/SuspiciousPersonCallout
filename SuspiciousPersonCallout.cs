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
        Vector3 shackSandyShoresWalkToLocation = new Vector3(1570.22f, 3696.93f, 34.3f);

        readonly Random rand = new Random();

        readonly string[] postActionStatements = { "Officer, I've done nothing wrong!", "Fuck the police!"};
        readonly WeaponHash[] weapons = { WeaponHash.Knife, WeaponHash.Pistol, WeaponHash.Machete, WeaponHash.Unarmed, WeaponHash.PumpShotgun};

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

            WeaponHash weapon = RandomizeWeapon(weapons);   // Get random weapon

            suspect.Weapons.Give(weapon, 100, false, true);     // Give weapon unholstered

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

            /* Wait for ped to get near then initiate contact*/
            while(!Game.PlayerPed.IsInRangeOf(ped.Position, 8f)) 
            {
                await BaseScript.Delay(1000);
            }
     
            ped.Task.TurnTo(Game.PlayerPed);
            await BaseScript.Delay(1000);

            // Vector3 currentLocation = suspect.Position; // Get current ped position

            PrintSubtitle("Hey officer", 2000);
            PrintSubtitle("Am I being detained?", 2000);
            await BaseScript.Delay(6000);   // Wait 6 seconds and then run to give player time to stop ped

            int number = rand.Next(101); // random integers between 0 and 100

            /* Randomize outcome */
            if (number >= 75)
            {
                Flee(ped);
            }
            else if (number >= 50)
            {
                FleeThenAttack(ped);
            }
            else if (number >= 25) 
            {
                Attack(ped);
            }
            else if (number >= 0)
            {
                Comply(ped);
            }
            else
            {
                Debug.WriteLine("Something went wrong with randomizing outcome.");  // Output to F8 Console
            }
        }
        private async void Flee(Ped ped) 
        {
            ped.Task.FleeFrom(Game.PlayerPed);  // Have Ped flee from player
            Debug.WriteLine("Fleeing");  // Output to F8 Console

            await BaseScript.Delay(1000);   // Wait a little after action

            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement,2000);
        }
        private async void FleeThenAttack(Ped ped) 
        {
            ped.Task.FleeFrom(Game.PlayerPed);

            await BaseScript.Delay(1500);   // Wait a little after action

            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement, 2000);

            await BaseScript.Delay(5000);   // Wait till behind the house

            ped.Task.FightAgainst(Game.PlayerPed);
            Debug.WriteLine("Fleeing and then Attacking");  // Output to F8 Console
        }
        private async void Attack(Ped ped) 
        {
            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement, 2000);

            await BaseScript.Delay(2500);   // Wait a little after statement

            ped.Task.FightAgainst(Game.PlayerPed);

            Debug.WriteLine("Attacking");  // Output to F8 Console
        }
        private void Comply(Ped ped) 
        {
            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement, 2000);

            Debug.WriteLine("Complying");  // Output to F8 Console
        }
        private String RandomizeStatements(String[] array) 
        {
            int index = rand.Next(array.Length);
            return array[index];
        }
        private WeaponHash RandomizeWeapon(WeaponHash[] array) 
        {
            int index = rand.Next(array.Length);
            return array[index];
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