using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CustomRegions.Mod;
using Menu;
using UnityEngine;

namespace CustomRegions.Arena
{
    internal static class ChallengeMenu
    {
        public static void ApplyHooks()
        {
            On.Menu.MultiplayerMenu.PopulateChallengeButtons += MultiplayerMenu_PopulateChallengeButtons;
            On.Menu.MultiplayerMenu.Singal += MultiplayerMenu_Singal;
            On.Menu.MultiplayerMenu.ClearGameTypeSpecificButtons += MultiplayerMenu_ClearGameTypeSpecificButtons;
            On.Menu.MultiplayerMenu.ShutDownProcess += MultiplayerMenu_ShutDownProcess;
            On.Menu.Menu.SliderSetValue += Menu_SliderSetValue;
            On.Menu.Menu.ValueOfSlider += Menu_ValueOfSlider;
        }

        private static float Menu_ValueOfSlider(On.Menu.Menu.orig_ValueOfSlider orig, Menu.Menu self, Slider slider)
        {
            if (self is MultiplayerMenu multi && slider == multi.challengeButtons().scroll)
            {
                return multi.challengeButtons().scrollPos / multi.challengeButtons().maxScroll;
            }
            return orig(self, slider);
        }

        private static void Menu_SliderSetValue(On.Menu.Menu.orig_SliderSetValue orig, Menu.Menu self, Slider slider, float f)
        {
            if (self is MultiplayerMenu multi && slider == multi.challengeButtons().scroll)
            {
                multi.challengeButtons().scrollPos = (multi.challengeButtons().maxScroll * f);
            }
            orig(self, slider, f);
        }

        private static void MultiplayerMenu_ShutDownProcess(On.Menu.MultiplayerMenu.orig_ShutDownProcess orig, MultiplayerMenu self)
        {
            orig(self);
            self.challengeButtons().ClearCRSButtons();
            self.challengeButtons().ClearMainButton();
        }

        private static void MultiplayerMenu_ClearGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_ClearGameTypeSpecificButtons orig, MultiplayerMenu self)
        {
            orig(self);
            self.challengeButtons().ClearCRSButtons();
            self.challengeButtons().ClearMainButton();
        }

        #region user interface
        private static void MultiplayerMenu_Singal(On.Menu.MultiplayerMenu.orig_Singal orig, MultiplayerMenu self, MenuObject sender, string message)
        {
            if (message == "CustomRegionsMode")
            {
                self.challengeButtons().CRSMode.toggled ^= true;
                self.challengeButtons().CRSPressed(self.challengeButtons().CRSMode.toggled);
            }

            else if (message.Contains("CHALLENGE"))
            {
                self.GetGameTypeSetup.CustomChallengeID().Value = null;
            }

            else if (message.Contains("crschallenge") && sender is SimpleButton button)
            {
                foreach (SimpleButton v in self.challengeButtons().challengeButtons)
                {
                    if (v != button)
                    { v.toggled = false; }
                }
                button.toggled = true;
                string[] array = message.Split('_');

                if (array.Length == 3 && int.TryParse(array[2], out int num))
                {
                    try
                    {
                        foreach (Structs.CustomChallenge customChallenge in ChallengeData.customChallenges)
                        {
                            if (customChallenge.id.value == array[1]) ChallengeData.currentChallenge = customChallenge;
                        }

                        self.GetGameTypeSetup.challengeID = num;
                        self.GetGameTypeSetup.CustomChallengeID().Value = ChallengeData.currentChallenge;

                        if (ChallengeData.currentChallenge is not null)
                        {
                            self.challengeInfo.RemoveSprites();
                            self.pages[0].RemoveSubObject(self.challengeInfo);

                            self.challengeInfo = new MoreSlugcats.ChallengeInformation(self, self.pages[0], num);

                            self.pages[0].subObjects.Add(self.challengeInfo);
                            self.playButton.inactive = !self.challengeInfo.unlocked;
                            self.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                        }

                    }
                    catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString()); self.GetGameTypeSetup.CustomChallengeID().Value = null; }

                    ChallengeData.currentChallenge = null;
                }
            }
            orig(self, sender, message);
        }

        private static void MultiplayerMenu_PopulateChallengeButtons(On.Menu.MultiplayerMenu.orig_PopulateChallengeButtons orig, MultiplayerMenu self)
        {
            if (self.challengeButtons().CRSMode == null)
            {
                self.challengeButtons().CRSMode = new SimpleButton(self, self.pages[0], "CRS", "CustomRegionsMode", new Vector2(180f, 540f), new Vector2(80f, 80f));
                self.pages[0].subObjects.Add(self.challengeButtons().CRSMode);
            }
            orig(self);
        }


        public class CRSChallengeButtons
        {
            private readonly MultiplayerMenu owner;

            public SimpleButton CRSMode = null;

            public VerticalSlider scroll = null;

            public LevelSelector.ScrollButton up = null;

            public LevelSelector.ScrollButton down = null;

            public List<CRSChallengeButton> challengeButtons = new();

            private const float dangerTop = 570;
            private const float dangerBottom = 338;

            private float _scrollPos = 0;

            public float scrollPos
            {
                get => _scrollPos;
                set {
                    UpdateScroll();
                    _scrollPos = value;
                }
            }

            public int maxScroll = 0;

            //public List<CRSChallengeButton> AllChallengeButtons => challengeButtons.Values.SelectMany(x => x).ToList();

            //public List<SimpleButton> challengeButtons = new();

            public CRSChallengeButtons(MultiplayerMenu owner)
            {
                this.owner = owner;
            }

            public void ClearVanillaButtons()
            {
                if (owner.challengeButtons != null)
                {
                    for (int i = 0; i < owner.challengeButtons.Length; i++)
                    {
                        owner.challengeButtons[i].RemoveSprites();
                        owner.pages[0].RemoveSubObject(owner.challengeButtons[i]);
                    }
                    owner.safariButtons = null;
                }
                if (owner.challengeChecks != null)
                {
                    for (int j = 0; j < owner.challengeChecks.Length; j++)
                    {
                        owner.challengeChecks[j].RemoveFromContainer();
                    }
                }
            }

            public void ClearMainButton()
            {
                if (CRSMode != null)
                {
                    CRSMode.RemoveSprites();
                    owner.pages[0].RemoveSubObject(CRSMode);
                    CRSMode = null;
                }
            }

            public void CRSPressed(bool CRSMode)
            {
                ClearVanillaButtons();
                ClearCRSButtons();

                if (CRSMode)
                {
                    InitCRSButtons();
                }
                else
                {
                    owner.PopulateChallengeButtons();
                }
            }

            public void ClearCRSButtons()
            {
                foreach (CRSChallengeButton button in challengeButtons)
                {
                    button.RemoveSprites();
                    owner.pages[0].RemoveSubObject(button);
                }

                if (scroll != null)
                {
                    scroll.RemoveSprites();
                    owner.pages[0].RemoveSubObject(scroll);
                }
                scroll = null;
                challengeButtons = new();
            }

            public void InitCRSButtons()
            {
                maxScroll = Math.Max(ChallengeData.customChallenges.Count - 5, 0);

                if (maxScroll > 0)
                {
                    scrollPos = maxScroll;
                    scroll = new(owner, owner.pages[0], "", new Vector2(270, 350), new Vector2(30, 250), new("crs"), false);
                    owner.pages[0].subObjects.Add(scroll);
                }

                int x = 277, y = 570;
                foreach (Structs.CustomChallenge data in ChallengeData.customChallenges)
                {
                    Vector3 vector = RWCustom.Custom.RGB2HSL(data.color);
                    HSLColor color = new (vector.x, vector.y, vector.z);

                    CRSChallengeButton label = new(owner, owner.pages[0], data.LocalizedID(owner), "crschallengelabel_" + data.id, new Vector2(x + 32, y), new Vector2(200, 52));
                    label.labelColor = color;

                    challengeButtons.Add(label);

                    owner.pages[0].subObjects.Add(label);

                    for (int i = 0; i < data.levels.Count(); i++)
                    {
                        var saveData = owner.manager.rainWorld.progression.miscProgressionData.CustomSaveData();
                        bool locked = !((data.unlocked || saveData.ChallengeTokenUnlocked(data.id))
                            && ChallengeRequirements.AllRequirementsFulfilled(data.UnlockRequirement[data.levels[i]], owner.multiplayerUnlocks));

                        CRSChallengeButton challenge = new(owner, owner.pages[0], (i + 1).ToString(), $"crschallenge_{data.id}_{i + 1}", new Vector2(x + ((4 + i) * 58), y), new Vector2(52, 52), locked);
                        challenge.labelColor = color;
                        challenge.fSprite.isVisible = owner.manager.rainWorld.progression.miscProgressionData.CustomSaveData().ChallengeComplete(data.id, i);

                        challengeButtons.Add(challenge);
                        owner.pages[0].subObjects.Add(challenge);
                    }
                    y -= 58;
                }
            }

            public void UpdateScroll()
            {
                foreach (CRSChallengeButton button in challengeButtons)
                {
                    button.ScrollUpdate(maxScroll - scrollPos);
                }
            }

            public class CRSChallengeButton : SimpleButton
            {
                public bool visible = true;

                public float lastAlpha = 0f;

                public float alpha = 1f;

                public Vector2 origPos;

                public FSprite fSprite;

                public MenuIllustration illustration;

                public CRSChallengeButton(Menu.Menu menu, MenuObject owner, string displayText, string singalText, Vector2 pos, Vector2 size, bool locked = false)
                    : base(menu, owner, locked ? "" : displayText, singalText, pos, size)
                {
                    origPos = pos;

                    if (locked)
                    {
                        inactive = true;
                        illustration = new MenuIllustration(menu, this, string.Empty, "Challenge_Locked", new Vector2(1f, 1f), true, false);
                        subObjects.Add(illustration);
                    }

                    fSprite = new("Menu_Symbol_CheckBox", true)
                    {
                        x = pos.x + size.x / 2f + 13f,
                        y = pos.y + size.y / 2f - 13f,
                        color = new Color(0.08235294f, 0.627451f, 0.3647059f),
                        isVisible = false
                    };

                    menu.pages[0].Container.AddChild(fSprite);

                    Update();
                    lastAlpha = alpha;
                    roundedRect.lasFillAplha = roundedRect.fillAlpha;
                    selectRect.lasFillAplha = selectRect.fillAlpha;
                    if (illustration != null) illustration.lastAlpha = illustration.alpha;
                }

                public void ScrollUpdate(float scroll)
                {
                    pos.y = origPos.y + scroll * 58f;
                    fSprite.y = pos.y + size.y / 2f - 13f;
                    //if (illustration != null) illustration.pos = pos + new Vector2(1f, 1f);
                }

                public override void RemoveSprites()
                {
                    base.RemoveSprites();
                    fSprite.RemoveFromContainer();
                }

                public override void Update()
                {
                    lastAlpha = alpha;

                    visible = pos.y <= dangerTop && pos.y >= dangerBottom;

                    if (visible && alpha < 1f)
                    {
                        alpha += 0.15f;
                    }
                    else if (!visible && alpha > 0f)
                    {
                        alpha -= 0.15f;
                    }

                    if (pos.y >= dangerTop)
                    { alpha = Mathf.Min(alpha, RWCustom.Custom.LerpMap(pos.y, dangerTop, dangerTop + 132, 1f, 0f)); }

                    if (pos.y <= dangerBottom)
                    { alpha = Mathf.Min(alpha, RWCustom.Custom.LerpMap(pos.y, dangerBottom, dangerBottom - 132, 1f, 0f)); }

                    alpha = Mathf.Clamp01(alpha);

                    base.Update();
                    roundedRect.fillAlpha *= alpha;
                    fSprite.alpha = alpha;

                    if (illustration != null) illustration.alpha = alpha;
                }

                public override void GrafUpdate(float timeStacker)
                {
                    base.GrafUpdate(timeStacker);
                    menuLabel.label.alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
                    for (int j = 9; j < 17; j++)
                    {
                        roundedRect.sprites[j].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        selectRect.sprites[j].alpha *= Mathf.Lerp(lastAlpha, alpha, timeStacker);
                    }
                }
            }
        }

        public static ConditionalWeakTable<MultiplayerMenu, CRSChallengeButtons> _challengeButtons = new();
        public static CRSChallengeButtons challengeButtons(this MultiplayerMenu p) => _challengeButtons.GetValue(p, _ => new(p));

        #endregion
    }
}
