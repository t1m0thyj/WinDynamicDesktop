// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Manina.Windows.Forms;

namespace WinDynamicDesktop
{
    public class ThemeListViewRenderer : ImageListView.ImageListViewRenderer
    {
        private readonly Size itemPadding = new Size(10, 10);
        private readonly Color hoveredColor = Color.FromArgb(229, 243, 255);
        private readonly Color focusedSelectedColor = Color.FromArgb(205, 232, 255);
        private readonly Color hoveredSelectedColor = Color.FromArgb(204, 232, 255);
        private readonly Color hoveredSelectedBorderColor = Color.FromArgb(153, 209, 255);
        private readonly Color unfocusedSelectedColor = Color.FromArgb(217, 217, 217);

        public override Size MeasureItem(View view)
        {
            Size itemSize = new Size();

            // Reference text height
            int textHeight = ImageListView.Font.Height;

            itemSize = ImageListView.ThumbnailSize + itemPadding + itemPadding;
            itemSize.Height += textHeight + System.Math.Max(4, textHeight / 3); // textHeight / 3 = vertical space between thumbnail and text

            return itemSize;
        }

        public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
        {
            Region oldClip = g.Clip;
            g.Clip = new Region(ClientBounds);

            // Paint background
            if ((state & ItemState.Selected) != ItemState.None)
            {
                if ((state & ItemState.Hovered) != ItemState.None)
                {
                    using (Brush bSelected = new SolidBrush(hoveredSelectedColor))
                    {
                        g.FillRectangle(bSelected, bounds);
                    }
                    using (Pen pSelected = new Pen(hoveredSelectedBorderColor))
                    {
                        g.DrawRectangle(pSelected, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                    }
                }
                else if (!ImageListView.Focused)
                {
                    using (Brush bSelected = new SolidBrush(unfocusedSelectedColor))
                    {
                        g.FillRectangle(bSelected, bounds);
                    }
                }
                else
                {
                    using (Brush bSelected = new SolidBrush(focusedSelectedColor))
                    {
                        g.FillRectangle(bSelected, bounds);
                    }
                }
            }
            else if ((state & ItemState.Hovered) != ItemState.None)
            {
                using (Brush bHovered = new SolidBrush(hoveredColor))
                {
                    g.FillRectangle(bHovered, bounds);
                }
            }

            // Draw the image
            Image img = item.GetCachedImage(CachedImageType.Thumbnail);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding,
                    ImageListView.ThumbnailSize));
                g.DrawImage(img, pos);
            }

            // Draw item text
            Color foreColor = ImageListView.Colors.ForeColor;
            if ((state & ItemState.Disabled) != ItemState.None)
            {
                foreColor = ImageListView.Colors.DisabledForeColor;
            }
            else if ((state & ItemState.Selected) != ItemState.None)
            {
                if (ImageListView.Focused)
                    foreColor = ImageListView.Colors.SelectedForeColor;
                else
                    foreColor = ImageListView.Colors.UnFocusedForeColor;
            }
            Size szt = System.Windows.Forms.TextRenderer.MeasureText(item.Text, ImageListView.Font);
            Rectangle rt = new Rectangle(bounds.Left + itemPadding.Width,
                bounds.Top + 2 * itemPadding.Height + ImageListView.ThumbnailSize.Height,
                ImageListView.ThumbnailSize.Width, szt.Height);
            System.Windows.Forms.TextRenderer.DrawText(g, item.Text, ImageListView.Font, rt, foreColor,
                System.Windows.Forms.TextFormatFlags.EndEllipsis |
                System.Windows.Forms.TextFormatFlags.HorizontalCenter |
                System.Windows.Forms.TextFormatFlags.VerticalCenter | System.Windows.Forms.TextFormatFlags.SingleLine);

            g.Clip = oldClip;
        }
    }
}
