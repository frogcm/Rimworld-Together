﻿using RimWorld;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameClient.Managers
{
    public static class WorldManager
    {
        public enum WorldStepMode { Required, Existing, Saved }

        public static void ParseWorldPacket(Packet packet)
        {
            WorldDetailsJSON worldDetailsJSON = Serializer.SerializeFromString<WorldDetailsJSON>(packet.contents[0]);

            switch (int.Parse(worldDetailsJSON.worldStepMode))
            {
                case (int)WorldStepMode.Required:
                    OnRequireWorld();
                    break;

                case (int)WorldStepMode.Existing:
                    OnExistingWorld(worldDetailsJSON);
                    break;

                case (int)WorldStepMode.Saved:
                    OnSavedWorld(worldDetailsJSON);
                    break;
            }
        }

        public static void OnRequireWorld()
        {
            DialogManager.PopWaitDialog();

            ClientValues.ToggleGenerateWorld(true);

            DialogManager.PushNewDialog(new Page_CreateWorldParams());

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "You are the first person joining the server!",
                "Configure the world that everyone will play on" });

            DialogManager.PushNewDialog(d1);
        }

        public static void OnExistingWorld(WorldDetailsJSON worldDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            ClientValues.ToggleLoadingPrefabWorld(true);

            WorldGeneratorManager.SetValuesFromServer(worldDetailsJSON);

            DialogManager.PushNewDialog(new Page_SelectScenario());

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "You are joining an existing server for the first time!",
                "Configure your playstyle to your liking", "Some settings might be disabled by the server" });

            DialogManager.PushNewDialog(d1);
        }

        public static void OnSavedWorld(WorldDetailsJSON worldDetailsJSON)
        {
            ClientValues.ToggleGenerateWorld(false);

            DialogManager.PopWaitDialog();

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(
                new string[] { "World file has been saved into the server!", "New connecting users will use this world when joining", "Press OK to start playing!" },
                delegate { OnExistingWorld(worldDetailsJSON);});

            DialogManager.PushNewDialog(d1);
        }
    }
}
