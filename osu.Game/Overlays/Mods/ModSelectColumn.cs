// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public abstract class ModSelectColumn : CompositeDrawable, IHasAccentColour
    {
        public readonly Container TopLevelContent;

        public LocalisableString HeaderText
        {
            set => createHeaderText(value);
        }

        public Color4 AccentColour
        {
            get => headerBackground.Colour;
            set => headerBackground.Colour = value;
        }

        /// <summary>
        /// Determines whether this column should accept user input.
        /// </summary>
        public readonly Bindable<bool> Active = new BindableBool(true);

        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) => base.ReceivePositionalInputAtSubTree(screenSpacePos) && Active.Value;

        protected readonly Container ControlContainer;
        protected readonly FillFlowContainer ItemsFlow;

        private readonly TextFlowContainer headerText;
        private readonly Box headerBackground;
        private readonly Container contentContainer;
        private readonly Box contentBackground;

        private const float header_height = 42;

        protected ModSelectColumn()
        {
            Width = 320;
            RelativeSizeAxes = Axes.Y;
            Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0);

            InternalChildren = new Drawable[]
            {
                TopLevelContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = ModSelectPanel.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = header_height + ModSelectPanel.CORNER_RADIUS,
                            Children = new Drawable[]
                            {
                                headerBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = header_height + ModSelectPanel.CORNER_RADIUS
                                },
                                headerText = new OsuTextFlowContainer(t =>
                                {
                                    t.Font = OsuFont.TorusAlternate.With(size: 17);
                                    t.Shadow = false;
                                    t.Colour = Colour4.Black;
                                })
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 17,
                                        Bottom = ModSelectPanel.CORNER_RADIUS
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = header_height },
                            Child = contentContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = ModSelectPanel.CORNER_RADIUS,
                                BorderThickness = 3,
                                Children = new Drawable[]
                                {
                                    contentBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension()
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                ControlContainer = new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Padding = new MarginPadding { Horizontal = 14 }
                                                }
                                            },
                                            new Drawable[]
                                            {
                                                new OsuScrollContainer(Direction.Vertical)
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    ClampExtension = 100,
                                                    ScrollbarOverlapsContent = false,
                                                    Child = ItemsFlow = new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Spacing = new Vector2(0, 7),
                                                        Padding = new MarginPadding(7)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private void createHeaderText(LocalisableString text)
        {
            headerText.Clear();

            int wordIndex = 0;

            headerText.AddText(text, t =>
            {
                if (wordIndex == 0)
                    t.Font = t.Font.With(weight: FontWeight.SemiBold);
                wordIndex += 1;
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            contentContainer.BorderColour = ColourInfo.GradientVertical(colourProvider.Background4, colourProvider.Background3);
            contentBackground.Colour = colourProvider.Background4;
        }
    }
}
