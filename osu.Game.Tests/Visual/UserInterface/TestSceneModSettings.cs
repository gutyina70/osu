// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModSettings : OsuManualInputManagerTestScene
    {
        private TestModSelectOverlay modSelect;

        private readonly Mod testCustomisableMod = new TestModCustomisable1();

        private readonly Mod testCustomisableAutoOpenMod = new TestModCustomisable2();

        private readonly Mod testCustomisableMenuCoveredMod = new TestModCustomisable1();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = Array.Empty<Mod>();
            Ruleset.Value = new TestRulesetInfo();
        });

        [Test]
        public void TestButtonShowsOnCustomisableMod()
        {
            createModSelect();
            openModSelect();

            AddAssert("button disabled", () => !modSelect.CustomiseButton.Enabled.Value);
            AddUntilStep("wait for button load", () => modSelect.ButtonsLoaded);
            AddStep("select mod", () => modSelect.SelectMod(testCustomisableMod));
            AddAssert("button enabled", () => modSelect.CustomiseButton.Enabled.Value);
            AddStep("open Customisation", () => modSelect.CustomiseButton.Click());
            AddStep("deselect mod", () => modSelect.SelectMod(testCustomisableMod));
            AddAssert("controls hidden", () => modSelect.ModSettingsContainer.Alpha == 0);
        }

        [Test]
        public void TestButtonShowsOnModAlreadyAdded()
        {
            AddStep("set active mods", () => SelectedMods.Value = new List<Mod> { testCustomisableMod });

            createModSelect();

            AddAssert("mods still active", () => SelectedMods.Value.Count == 1);

            openModSelect();
            AddAssert("button enabled", () => modSelect.CustomiseButton.Enabled.Value);
        }

        [Test]
        public void TestCustomisationMenuVisibility()
        {
            createModSelect();
            openModSelect();

            AddAssert("Customisation closed", () => modSelect.ModSettingsContainer.Alpha == 0);
            AddStep("select mod", () => modSelect.SelectMod(testCustomisableAutoOpenMod));
            AddAssert("Customisation opened", () => modSelect.ModSettingsContainer.Alpha == 1);
            AddStep("deselect mod", () => modSelect.SelectMod(testCustomisableAutoOpenMod));
            AddAssert("Customisation closed", () => modSelect.ModSettingsContainer.Alpha == 0);
        }

        [Test]
        public void TestModSettingsUnboundWhenCopied()
        {
            OsuModDoubleTime original = null;
            OsuModDoubleTime copy = null;

            AddStep("create mods", () =>
            {
                original = new OsuModDoubleTime();
                copy = (OsuModDoubleTime)original.CreateCopy();
            });

            AddStep("change property", () => original.SpeedChange.Value = 2);

            AddAssert("original has new value", () => Precision.AlmostEquals(2.0, original.SpeedChange.Value));
            AddAssert("copy has original value", () => Precision.AlmostEquals(1.5, copy.SpeedChange.Value));
        }

        [Test]
        public void TestCustomisationMenuNoClickthrough()
        {
            createModSelect();
            openModSelect();

            AddStep("change mod settings menu width to full screen", () => modSelect.SetModSettingsWidth(1.0f));
            AddStep("select cm2", () => modSelect.SelectMod(testCustomisableAutoOpenMod));
            AddAssert("Customisation opened", () => modSelect.ModSettingsContainer.Alpha == 1);
            AddStep("hover over mod behind settings menu", () => InputManager.MoveMouseTo(modSelect.GetModButton(testCustomisableMenuCoveredMod)));
            AddAssert("Mod is not considered hovered over", () => !modSelect.GetModButton(testCustomisableMenuCoveredMod).IsHovered);
            AddStep("left click mod", () => InputManager.Click(MouseButton.Left));
            AddAssert("only cm2 is active", () => SelectedMods.Value.Count == 1);
            AddStep("right click mod", () => InputManager.Click(MouseButton.Right));
            AddAssert("only cm2 is active", () => SelectedMods.Value.Count == 1);
        }

        private void createModSelect()
        {
            AddStep("create mod select", () =>
            {
                Child = modSelect = new TestModSelectOverlay
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    SelectedMods = { BindTarget = SelectedMods }
                };
            });
        }

        private void openModSelect()
        {
            AddStep("open", () => modSelect.Show());
            AddUntilStep("wait for ready", () => modSelect.State.Value == Visibility.Visible && modSelect.ButtonsLoaded);
        }

        private class TestModSelectOverlay : ModSelectOverlay
        {
            public new Container ModSettingsContainer => base.ModSettingsContainer;
            public new TriangleButton CustomiseButton => base.CustomiseButton;

            public bool ButtonsLoaded => ModSectionsContainer.Children.All(c => c.ModIconsLoaded);

            public ModButton GetModButton(Mod mod)
            {
                return ModSectionsContainer.Children.Single(s => s.ModType == mod.Type)
                                           .ButtonsContainer.OfType<ModButton>().Single(b => b.Mods.Any(m => m.GetType() == mod.GetType()));
            }

            public void SelectMod(Mod mod) =>
                GetModButton(mod).SelectNext(1);

            public float SetModSettingsWidth(float newWidth)
            {
                float oldWidth = ModSettingsContainer.Width;
                ModSettingsContainer.Width = newWidth;
                return oldWidth;
            }
        }

        public class TestRulesetInfo : RulesetInfo
        {
            public override Ruleset CreateInstance() => new TestCustomisableModRuleset();

            public TestRulesetInfo()
            {
                Available = true;
            }

            public class TestCustomisableModRuleset : Ruleset
            {
                public override IEnumerable<Mod> GetModsFor(ModType type)
                {
                    if (type == ModType.Conversion)
                    {
                        return new Mod[]
                        {
                            new TestModCustomisable1(),
                            new TestModCustomisable2()
                        };
                    }

                    return Array.Empty<Mod>();
                }

                public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new NotImplementedException();

                public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();

                public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => throw new NotImplementedException();

                public override string Description { get; } = "test";
                public override string ShortName { get; } = "tst";
            }
        }

        private class TestModCustomisable1 : TestModCustomisable
        {
            public override string Name => "Customisable Mod 1";

            public override string Acronym => "CM1";
        }

        private class TestModCustomisable2 : TestModCustomisable
        {
            public override string Name => "Customisable Mod 2";

            public override string Acronym => "CM2";

            public override bool RequiresConfiguration => true;
        }

        private abstract class TestModCustomisable : Mod, IApplicableMod
        {
            public override double ScoreMultiplier => 1.0;

            public override ModType Type => ModType.Conversion;

            [SettingSource("Sample float", "Change something for a mod")]
            public BindableFloat SliderBindable { get; } = new BindableFloat
            {
                MinValue = 0,
                MaxValue = 10,
                Default = 5,
                Value = 7
            };

            [SettingSource("Sample bool", "Clicking this changes a setting")]
            public BindableBool TickBindable { get; } = new BindableBool();
        }
    }
}
