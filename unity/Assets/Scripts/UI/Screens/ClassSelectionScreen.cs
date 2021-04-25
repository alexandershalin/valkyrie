﻿using System.Collections.Generic;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Screens
{
    class ClassSelectionScreen
    {
        private const int LARGE_FONT_LIMIT = 24;
        protected List<float> scrollOffset = new List<float>();
        protected List<UIElementScrollVertical> scrollArea = new List<UIElementScrollVertical>();

        public ClassSelectionScreen()
        {
            Draw();
        }

        public void Draw()
        {
            // Clean up
            Destroyer.Dialog();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.HEROSELECT))
                Object.Destroy(go);

            Game game = Game.Get();

            // Add a title to the page
            UIElement ui = new UIElement(Game.HEROSELECT);
            ui.SetLocation(8, 1, UIScaler.GetWidthUnits() - 16, 3);
            ui.SetText(new StringKey("val","SELECT_CLASS"));
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());

            // Get all heros
            int heroCount = 0;
            // Count number of selected heroes
            foreach (Quest.Hero h in game.CurrentQuest.heroes)
            {
                if (h.heroData != null) heroCount++;
            }

            float xOffset = UIScaler.GetHCenter(-18);
            if (heroCount < 4) xOffset += 4.5f;
            if (heroCount < 3) xOffset += 4.5f;

            for (int i = 0; i < heroCount; i++)
            {
                DrawHero(xOffset, i);
                xOffset += 9f;
            }

            // Add a finished button to start the quest
            ui = new UIElement(Game.HEROSELECT);
            ui.SetLocation(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-2.5f), 8, 2);
            ui.SetText(CommonStringKeys.FINISHED, Color.green);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Finished);
            new UIElementBorder(ui, Color.green);

            ui = new UIElement(Game.HEROSELECT);
            ui.SetLocation(0.5f, UIScaler.GetBottom(-2.5f), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(BackButtonAction());
            new UIElementBorder(ui, Color.red);
        }

        private static UnityAction BackButtonAction()
        {
            Game game = Game.Get();
            if (game.testMode)
            {
                return GameStateManager.Editor.EditCurrentQuest;
            }
            return GameStateManager.Quest.RestartFromHeroSelection;
        }

        public void DrawHero(float xOffset, int hero)
        {
            Game game = Game.Get();

            if (scrollOffset.Count > hero)
            {
                scrollOffset[hero] = scrollArea[hero].GetScrollPosition();
            }

            string archetype = game.CurrentQuest.heroes[hero].heroData.archetype;
            string hybridClass = game.CurrentQuest.heroes[hero].hybridClass;
            float yStart = 7f;

            UIElement ui = null;
            if (hybridClass.Length > 0)
            {
                ClassData classData = game.cd.Get<ClassData>(hybridClass);
                archetype = classData.hybridArchetype;
                ui = new UIElement(Game.HEROSELECT);
                ui.SetLocation(xOffset + 0.25f, yStart, 8.5f, 5);
                new UIElementBorder(ui);

                ui = new UIElement(Game.HEROSELECT);
                ui.SetLocation(xOffset + 1, yStart + 0.5f, 7, 4);
                ui.SetText(classData.name, Color.black);
                if (ui.GetText().Length > LARGE_FONT_LIMIT)
                {
                    ui.SetFontSize(UIScaler.GetSmallFont());
                }
                else
                {
                    ui.SetFontSize(UIScaler.GetMediumFont());
                }
                ui.SetButton(delegate { Select(hero, hybridClass); });
                ui.SetBGColor(new Color(0, 0.7f, 0));
                new UIElementBorder(ui, Color.black);

                yStart += 5;
            }

            while (scrollArea.Count <= hero)
            {
                scrollArea.Add(null);
            }
            scrollArea[hero] = new UIElementScrollVertical(Game.HEROSELECT);
            scrollArea[hero].SetLocation(xOffset + 0.25f, yStart, 8.5f, 27 - yStart);
            new UIElementBorder(scrollArea[hero]);

            float yOffset = 1;

            foreach (ClassData cd in game.cd.Values<ClassData>())
            {
                if (!cd.archetype.Equals(archetype)) continue;
                if (cd.hybridArchetype.Length > 0 && hybridClass.Length > 0) continue;

                string className = cd.sectionName;
                bool available = true;
                bool pick = false;

                for (int i = 0; i < game.CurrentQuest.heroes.Count; i++)
                {
                    if (game.CurrentQuest.heroes[i].className.Equals(className))
                    {
                        available = false;
                        if (hero == i)
                        {
                            pick = true;
                        }
                    }
                    if (game.CurrentQuest.heroes[i].hybridClass.Equals(className))
                    {
                        available = false;
                    }
                }

                ui = new UIElement(Game.HEROSELECT, scrollArea[hero].GetScrollTransform());
                ui.SetLocation(0.25f, yOffset, 7, 4);
                if (available)
                {
                    ui.SetBGColor(Color.white);
                    ui.SetButton(delegate { Select(hero, className); });
                }
                else
                {
                    ui.SetBGColor(new Color(0.2f, 0.2f, 0.2f));
                    if (pick)
                    {
                        ui.SetBGColor(new Color(0, 0.7f, 0));
                    }
                }
                ui.SetText(cd.name, Color.black);
                if (ui.GetText().Length > LARGE_FONT_LIMIT)
                {
                    ui.SetFontSize(UIScaler.GetSmallFont());
                }
                else
                {
                    ui.SetFontSize(UIScaler.GetMediumFont());
                }

                yOffset += 5f;
            }

            scrollArea[hero].SetScrollSize(yOffset);
            if (scrollOffset.Count > hero)
            {
                scrollArea[hero].SetScrollPosition(scrollOffset[hero]);
            }
            else
            {
                scrollOffset.Add(0);
            }

            Texture2D heroTex = ContentData.FileToTexture(game.CurrentQuest.heroes[hero].heroData.image);
            Sprite heroSprite = Sprite.Create(heroTex, new Rect(0, 0, heroTex.width, heroTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.HEROSELECT);
            ui.SetLocation(xOffset + 2.5f, 3.5f, 4, 4);
            ui.SetImage(heroSprite);
        }

        public void Select(int hero, string className)
        {
            Game game = Game.Get();
            if (game.cd.Get<ClassData>(className).hybridArchetype.Length > 0)
            {
                game.CurrentQuest.heroes[hero].className = "";
                if (game.CurrentQuest.heroes[hero].hybridClass.Length > 0)
                {
                    game.CurrentQuest.heroes[hero].hybridClass = "";
                }
                else
                {
                    game.CurrentQuest.heroes[hero].hybridClass = className;
                }
            }
            else
            {
                game.CurrentQuest.heroes[hero].className = className;
            }
            Draw();
        }

        public void Finished()
        {
            Game game = Game.Get();

            HashSet<string> items = new HashSet<string>();
            foreach (Quest.Hero h in game.CurrentQuest.heroes)
            {
                if (h.heroData == null) continue;
                if (h.className.Length == 0) return;

                game.CurrentQuest.vars.SetValue("#" + h.className, 1);

                foreach (string s in game.cd.Get<ClassData>(h.className).items)
                {
                    items.Add(s);
                }

                if (h.hybridClass.Length == 0) continue;
                foreach (string s in game.cd.Get<ClassData>(h.hybridClass).items)
                {
                    items.Add(s);
                }
            }
            game.CurrentQuest.items.UnionWith(items);

            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.HEROSELECT))
                Object.Destroy(go);
            game.moraleDisplay = new MoraleDisplay();
            game.QuestStartEvent();
        }
    }
}
