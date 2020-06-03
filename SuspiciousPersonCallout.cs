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
        Vector3 selectedSpawnLocation;
        Vector3 selectedRunToLocation;

        readonly Vector3 shackSandyShoresLocation = new Vector3(1704.28f, 3845.39f, 34.94f);
        readonly Vector3 shackSandyShoresRunToLocation = new Vector3(1709.09f, 3832.57f, 34.99f);

        readonly Vector3 complexSandyShoresLocation = new Vector3(1545.45f, 3592.52f, 35.45f);
        readonly Vector3 complexSandyShoresRunToLocation = new Vector3(1529.04f, 3592.92f, 35.41f);

        readonly Random rand = new Random();

        readonly string[] postActionStatements = { "~r~[Suspect] Officer, I've done nothing wrong!", "~r~[Suspect] Fuck the police!" };
        readonly WeaponHash[] weapons = { WeaponHash.Knife, WeaponHash.Pistol, WeaponHash.Machete, WeaponHash.Unarmed, WeaponHash.PumpShotgun};

        public SuspiciousPersonCallout()
        {
            RandomizeSpawn();

            InitBase(selectedSpawnLocation);

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
            PrintNotification("~g~[Dispatch]: ~w~There are reports of a suspicious person trespassing into an abandoned building");

            /* Use the SpawnPed or SpawnVehicle method to get a properly networked ped (react to other players) */
            suspect = await SpawnPed(PedHash.ChiCold01GMM, selectedSpawnLocation, 300f);

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

            StartScene();
        }
        private async void StartScene() 
        {
            await BaseScript.Delay(6000);   // Wait for player to reach close to location

            suspect.Task.RunTo(selectedRunToLocation);    // Have Ped Run to position on map with cords

            await BaseScript.Delay(3000);

            suspect.Task.WanderAround();

            /* Wait for ped to get near then initiate contact*/
            while (!Game.PlayerPed.IsInRangeOf(suspect.Position, 8f)) 
            {
                await BaseScript.Delay(1000);
            }
     
            suspect.Task.TurnTo(Game.PlayerPed);
            await BaseScript.Delay(1000);

            PrintSubtitle("~r~[Suspect] Hey officer", 2000);
            await BaseScript.Delay(2000);
            PrintSubtitle("~r~[Suspect] Why are you stopping me?", 2000);
            await BaseScript.Delay(6000);   // Wait 6 seconds and then do action to give player time to stop ped

            int number = rand.Next(101); // random integers between 0 and 100

            /* Randomize outcome */
            if (number >= 75)
            {
                Flee();
            }
            else if (number >= 50)
            {
                FleeThenAttack();
            }
            else if (number >= 25) 
            {
                Attack();
            }
            else if (number >= 0)
            {
                Comply();
            }
            else
            {
                Debug.WriteLine("Something went wrong with randomizing outcome.");  // Output to F8 Console
            }
        }
        public override void OnCancelBefore()
        {
            if (suspect.IsAlive)
            {
                suspect.Task.WanderAround();
            }
        }
        private async void Flee() 
        {
            suspect.Task.FleeFrom(Game.PlayerPed);  // Have Ped flee from player

            await BaseScript.Delay(1000);   // Wait a little after action

            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement,2000);
        }
        private async void FleeThenAttack() 
        {
            suspect.Task.FleeFrom(Game.PlayerPed);

            await BaseScript.Delay(1500);   // Wait a little after action

            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement, 2000);

            await BaseScript.Delay(5000);   // Wait till behind the house

            suspect.Task.FightAgainst(Game.PlayerPed);
        }
        private async void Attack() 
        {
            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement, 2000);

            await BaseScript.Delay(2500);   // Wait a little after statement

            suspect.Task.FightAgainst(Game.PlayerPed);
        }
        private void Comply() 
        {
            String statement = RandomizeStatements(postActionStatements);   // Receive random statement
            PrintSubtitle(statement, 2000);
        }
        private void RandomizeSpawn() 
        {
            /* Randomize spawn location */
            int number = rand.Next(101); // random integers between 0 and 100

            if (number >= 50)
            {
                selectedSpawnLocation = shackSandyShoresLocation;
                selectedRunToLocation = shackSandyShoresRunToLocation;
            }
            else if (number >= 0)
            {
                selectedSpawnLocation = complexSandyShoresLocation;
                selectedRunToLocation = complexSandyShoresRunToLocation;
            }
            else
            {
                Debug.WriteLine("Something went wrong with randomizing spawn.");  // Output to F8 Console
            }
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