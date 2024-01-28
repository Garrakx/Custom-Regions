using CustomRegions.Mod;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomRegions.Arena
{
    internal static class ChallengeToken
    {
        private static ConditionalWeakTable<CollectToken.CollectTokenData, StrongBox<bool>> _IsPurple = new();

        public static ChallengeData.ChallengeUnlockID GetChallengeUnlock(this CollectToken.CollectTokenData p) {
            if (!p.GetIsPurple() || p.tokenString == null || p.tokenString.Length < 1)
            {
                return null;
            }
            return new ChallengeData.ChallengeUnlockID(p.tokenString, false);
        }

        public static void SetChallengeUnlock(this CollectToken.CollectTokenData p, ChallengeData.ChallengeUnlockID value) {
            if (!p.GetIsPurple()) return;
            
            if (value == null)
            {
                p.tokenString = string.Empty;
                return;
            }
            p.tokenString = value.value;
        }

        public static bool GetIsPurple(this CollectToken.CollectTokenData p) => _IsPurple.GetValue(p, _ => new(false)).Value;
        public static void SetIsPurple(this CollectToken.CollectTokenData p, bool value) => _IsPurple.GetValue(p, _ => new(false)).Value = value;

        public static bool IsPurple(this CollectToken p) => (p.placedObj.data as CollectToken.CollectTokenData).GetIsPurple();

        public static HSLColor PurpleColor => new HSLColor(0.78f, 1f, 0.5f);

        public static void ApplyHooks()
        {
            //devtools
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider.ctor += IndexControlSlider_ctor;
            On.DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider.NubDragged += IndexControlSlider_NubDragged;
            On.DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider.Refresh += IndexControlSlider_Refresh;
            On.DevInterface.TokenRepresentation.TokenControlPanel.UpdateTokenText += TokenControlPanel_UpdateTokenText;
            On.DevInterface.TokenRepresentation.TokenName += TokenRepresentation_TokenName;

            //graphics
            On.CollectToken.InitiateSprites += CollectToken_InitiateSprites;
            On.CollectToken.AddToContainer += CollectToken_AddToContainer;
            On.CollectToken.DrawSprites += CollectToken_DrawSprites;
            On.CollectToken.GoldCol += CollectToken_GoldCol;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            try { new Hook(typeof(CollectToken).GetProperty(nameof(CollectToken.TokenColor), flags).GetGetMethod(), ChallengeToken.TokenColorHook); }
            catch (Exception e) { CustomRegionsMod.BepLog($"exception when hooking CollectToken.TokenColor\n{e}"); }

            //function
            On.Room.Loaded += Room_Loaded;
            On.CollectToken.Pop += CollectToken_Pop;
            On.CollectToken.Update += CollectToken_Update;
        }

        #region function
        private static void CollectToken_Update(On.CollectToken.orig_Update orig, CollectToken self, bool eu)
        {
            orig(self, eu);
            if (self.expandAroundPlayer != null && self.contract && self.expand < 0f && self.anythingUnlocked && self.room.game.cameras[0].hud != null && self.room.game.cameras[0].hud.textPrompt != null && self.IsPurple())
            {
                for (int i = 0; i < self.room.game.cameras[0].hud.textPrompt.messages.Count; i++)
                {
                    if (self.room.game.cameras[0].hud.textPrompt.messages[i].text == self.room.game.manager.rainWorld.inGameTranslator.Translate("New arenas unlocked"))
                    {
                        self.room.game.cameras[0].hud.textPrompt.messages.RemoveAt(i);
                        break;
                    }
                }
                self.room.game.cameras[0].hud.textPrompt.AddMessage(self.room.game.manager.rainWorld.inGameTranslator.Translate("Challenge unlocked:") + " " + self.room.game.manager.rainWorld.inGameTranslator.Translate((self.placedObj.data as CollectToken.CollectTokenData).GetChallengeUnlock().ToString()), 20, 160, true, true);
            }
        }

        private static void CollectToken_Pop(On.CollectToken.orig_Pop orig, CollectToken self, Player player)
        {
            orig(self, player);
            var data = (self.placedObj.data as CollectToken.CollectTokenData);
            var progression = self.room.game.rainWorld.progression.miscProgressionData;
            if (data.GetIsPurple() && data.GetChallengeUnlock() != null)
            {
                self.anythingUnlocked = !progression.CustomSaveData().ChallengeTokenUnlocked(data.GetChallengeUnlock());
                progression.CustomSaveData().ChallengeTokenCollected(data.GetChallengeUnlock());
            }
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);

            if (self.game == null) return;
            for (int num9 = 0; num9 < self.roomSettings.placedObjects.Count; num9++)
            {
                if (!self.roomSettings.placedObjects[num9].active) continue;
                if (self.roomSettings.placedObjects[num9].type == PurpleToken && !(ModManager.Expedition && self.game.rainWorld.ExpeditionMode))
                {
                    ChallengeData.ChallengeUnlockID id = new((self.roomSettings.placedObjects[num9].data as CollectToken.CollectTokenData).tokenString, false);
                    if (self.game.session is not StoryGameSession || self.world.singleRoomWorld || !(self.game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.CustomSaveData().ChallengeTokenUnlocked(id))
                    {
                        self.AddObject(new CollectToken(self, self.roomSettings.placedObjects[num9]));
                    }
                    else
                    {
                        self.AddObject(new CollectToken.TokenStalk(self, self.roomSettings.placedObjects[num9].pos, self.roomSettings.placedObjects[num9].pos + (self.roomSettings.placedObjects[num9].data as CollectToken.CollectTokenData).handlePos, null, false));
                    }
                }
            }
        }
        #endregion

        #region graphics
        static Color TokenColorHook(Func<CollectToken, Color> orig, CollectToken self)
        {
            if (self.IsPurple()) return PurpleColor.rgb;
            else return orig(self);
        }

        private static Color CollectToken_GoldCol(On.CollectToken.orig_GoldCol orig, CollectToken self, float g)
        {
            if (self.IsPurple()) return Color.Lerp(self.TokenColor, new Color(1f, 1f, 1f), 0.4f + 0.4f * Mathf.Max(self.contract ? 0.5f : (self.expand * 0.5f), Mathf.Pow(g, 0.5f)));
            else return orig(self, g);
        }

        private static void CollectToken_DrawSprites(On.CollectToken.orig_DrawSprites orig, CollectToken self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.IsPurple())
            {
                float num = Mathf.Lerp(self.lastGlitch, self.glitch, timeStacker);
                float num2 = Mathf.Lerp(self.lastExpand, self.expand, timeStacker);
                float num3 = Mathf.Lerp(self.lastPower, self.power, timeStacker);
                if (self.room != null && !self.AvailableToPlayer())
                {
                    num = Mathf.Lerp(num, 1f, UnityEngine.Random.value);
                    num3 *= 0.3f + 0.7f * UnityEngine.Random.value;
                }
                sLeaser.sprites[self.GoldSprite].alpha = 0.75f * Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * UnityEngine.Random.value)), 0.7f, num2) * num3;
            }
        }

        private static void CollectToken_AddToContainer(On.CollectToken.orig_AddToContainer orig, CollectToken self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            newContatiner ??= rCam.ReturnFContainer("Water");
            if (self.IsPurple()) newContatiner.AddChild(sLeaser.sprites[self.GoldSprite]);
        }

        private static void CollectToken_InitiateSprites(On.CollectToken.orig_InitiateSprites orig, CollectToken self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.IsPurple())
            {
                sLeaser.sprites[self.GoldSprite].color = Color.Lerp(new Color(0f, 0f, 0f), RainWorld.GoldRGB, 0.2f);
                sLeaser.sprites[self.GoldSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
            }
        }

        #endregion

        #region devinterface
        private static string TokenRepresentation_TokenName(On.DevInterface.TokenRepresentation.orig_TokenName orig, CollectToken.CollectTokenData data)
        {
            if (data.GetIsPurple()) return "Purple Token";
            else return orig(data);
        }

        private static void TokenControlPanel_UpdateTokenText(On.DevInterface.TokenRepresentation.TokenControlPanel.orig_UpdateTokenText orig, DevInterface.TokenRepresentation.TokenControlPanel self)
        {
            orig(self);
            if (self.TokenData.GetIsPurple())
            {
                if (self.TokenData.GetChallengeUnlock() == null)
                {
                    self.lbl.Text = "Undefined Challenge";
                    return;
                }
                self.lbl.Text = self.TokenData.GetChallengeUnlock().value;
                return;
            }
        }

        private static void IndexControlSlider_Refresh(On.DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider.orig_Refresh orig, DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider self)
        {
            orig(self);
            if (self.TokenData.GetIsPurple() && self.TokenData.GetChallengeUnlock() != null)
            {
                int num = self.TokenData.GetChallengeUnlock().Index;
                self.NumberText = num.ToString();
                self.RefreshNubPos((float)num / (float)self.maxNubInt);
            }
        }

        private static void IndexControlSlider_NubDragged(On.DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider.orig_NubDragged orig, DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider self, float nubPos)
        {
            orig(self, nubPos);

            if (self.IDstring != null && self.TokenData.GetIsPurple())
            {
                string entry4 = ExtEnum<ChallengeData.ChallengeUnlockID>.values.GetEntry(Mathf.FloorToInt(nubPos * (float)self.maxNubInt));
                if (entry4 != null)
                {
                    self.TokenData.SetChallengeUnlock(new ChallengeData.ChallengeUnlockID(entry4, false));
                }
                self.parentNode.parentNode.Refresh();
                (self.parentNode as DevInterface.TokenRepresentation.TokenControlPanel).UpdateTokenText();
                self.Refresh();
            }
        }

        private static void IndexControlSlider_ctor(On.DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider.orig_ctor orig, DevInterface.TokenRepresentation.TokenControlPanel.IndexControlSlider self, DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, Vector2 pos, string title)
        {
            orig(self, owner, IDstring, parentNode, pos, title);

            if (self.TokenData.GetIsPurple())
                self.maxNubInt = ExtEnum<ChallengeData.ChallengeUnlockID>.values.Count - 1;
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, DevInterface.ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            orig(self, tp, pObj);

            if (tp == PurpleToken)
            {
                if (pObj == null)
                {
                    pObj = new PlacedObject(tp, null);
                    pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + RWCustom.Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
                    self.RoomSettings.placedObjects.Add(pObj);
                    if (tp == PlacedObject.Type.LightFixture)
                    {
                        (pObj.data as PlacedObject.LightFixtureData).type = self.lastPlacedLightFixture;
                    }
                }
                DevInterface.PlacedObjectRepresentation placedObjectRepresentation = new DevInterface.TokenRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj);
                self.tempNodes.Add(placedObjectRepresentation);
                self.subNodes.Add(placedObjectRepresentation);
            }
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            orig(self);
            if (self.type == PurpleToken)
            {
                self.data = new CollectToken.CollectTokenData(self, false);
                (self.data as CollectToken.CollectTokenData).SetIsPurple(true);
            }
        }

        private static DevInterface.ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, DevInterface.ObjectsPage self, PlacedObject.Type type)
        {
            if (type == PurpleToken) return DevInterface.ObjectsPage.DevObjectCategories.Tutorial;
            else return orig(self, type);
        }
        #endregion

        public static PlacedObject.Type PurpleToken = new("PurpleToken", true);
    }
}
