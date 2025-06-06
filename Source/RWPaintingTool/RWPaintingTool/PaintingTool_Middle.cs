﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWPaintingTool
{
    public partial class PaintingTool
    {
        public void DrawMiddlePart(Rect inRect)
        {

            inRect.SplitHorizontally(ButSize.y, out var apparelButtonRect, out inRect);
            Widgets.Dropdown(apparelButtonRect, _thing, (p) => p, MenuGenerator, _thing?.Label, _thing?.UIIconOverride as Texture2D);
            inRect.SplitHorizontallyWithMargin(out var pawnRect, out var bottomButtonsRect, out _, bottomHeight: ButSize.y * 3 + Margin * 2);
            DrawPawn(pawnRect);


            Rect rect2 = bottomButtonsRect;
            var y = rect2.y;
            Widgets.CheckboxLabeled(new Rect(rect2.x, y, rect2.width, ButSize.y), "ShowHeadgear".Translate(), ref this.showHeadgear, false, null, null, false, false);
            y += ButSize.y;
            Widgets.CheckboxLabeled(new Rect(rect2.x, y, rect2.width, ButSize.y), "RWPT_ShowSelectedOnly".Translate(), ref this.showSelectedOnly, false, null, null, false, false);
            y += ButSize.y + Margin;
            if (Widgets.ButtonText(new Rect(rect2.x, y, rect2.width, ButSize.y), "Accept".Translate()))
            {
                Accept();
            }


        }

        private void Accept()
        {
            foreach (var tracker in PaintableApparel.Select(ColorTrackerDB.GetTracker))
            {
                tracker.Commit();
            }
            this.Close();
        }

        private IEnumerable<Apparel> PaintableApparel => pawn.apparel.WornApparel.Where(x => x.def.HasModExtension<PaintableExtension>());

        private IEnumerable<Widgets.DropdownMenuElement<Thing>> MenuGenerator(Thing thing)
        {
            foreach (var apparel in PaintableApparel)
            {
                yield return new Widgets.DropdownMenuElement<Thing>()
                {
                    option = new FloatMenuOption(apparel.Label, () => SetFor(apparel)),
                    payload = apparel
                };
            }
        }

        private void DrawPawn(Rect rect)
        {
            //Widgets.BeginGroup(rect);
            var colorTrackers = PaintableApparel.Select(a => ColorTrackerDB.GetTracker(a));

            foreach (var colorTracker in colorTrackers)
            {
                colorTracker.ShouldUseTemp = true;
            }
            Rect position = rect.ContractedBy(Margin);
            if (!showSelectedOnly)
            {
                RenderTexture image = PortraitsCache.Get(this.pawn, new Vector2(position.width, position.height), rotation, Dialog_StylingStation.PortraitOffset, 1.1f, true, true, this.showHeadgear, this.showClothes, this.apparelColors, null, true, null);
                GUI.DrawTexture(position, image);
            }
            else
            {
                GraphicsPatches.CurThing = _thing;
                ApparelGraphicRecordGetter.TryGetGraphicApparel(_thing as Apparel, pawn?.story?.bodyType, out var graphicRecord);
                var mat = graphicRecord.graphic.MatAt(rotation);
                GenUI.DrawTextureWithMaterial(position, mat.mainTexture, mat);
            }

            var leftArrowRect = new Rect(rect.xMin, rect.center.y, Margin, Margin);
            if (Widgets.ButtonText(leftArrowRect, "<", false))
            {
                rotation = new Rot4(rotation.AsInt - 1);
            }

            var rightArrowRect = new Rect(rect.xMax - Margin, rect.center.y, Margin, Margin);
            if (Widgets.ButtonText(rightArrowRect, ">", false))
            {
                rotation = new Rot4(rotation.AsInt + 1);
            }
            foreach (var colorTracker in colorTrackers)
            {
                colorTracker.ShouldUseTemp = false;
            }

            //Widgets.EndGroup();
        }
    }
}
