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
        Ped person;

        public SuspiciousPersonCallout()
        {
            //Random rnd = new Random();

            Vector3 spawnLocation = new Vector3(1585.8f, 3646.41f, 34.52f);

            InitBase(spawnLocation);


            ShortName = "Suspicious Person";
            CalloutDescription = "Caller reported about a suspicious person walking around.";
            ResponseCode = 2;
            StartDistance = 8f;
        }
        public async override Task Init()
        {
            /* Called when the callout is accepted */
            /* Blip spawn happens in base.OnAccept() */
            this.OnAccept();

            /* Use the SpawnPed or SpawnVehicle method to get a properly networked ped (react to other players) */
            person = await SpawnPed(PedHash.FosRepCutscene, this.Location, 210);

            person.Weapons.Give(WeaponHash.Pistol, 100, true, true);

            person.AlwaysKeepTask = true;
            person.BlockPermanentEvents = true;
        }
        public override void OnStart(Ped player)
        {
            /* Called when a player gets in range */

            base.OnStart(player); // -> to remove the blip from the map (yellow circle by default)

            // person.Task.FleeFrom(player); // Have Ped flee when player enters start position

            person.Task.GoTo(new Vector3(1613.26f, 3593.69f, 35.15f));    // Have Ped Walk to position on map with cords
        }
    }
}