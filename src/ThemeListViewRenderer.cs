using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using Manina.Windows.Forms;

namespace WinDynamicDesktop
{
    public class ThemeListViewRenderer : ImageListView.ImageListViewRenderer
    {
        public override Size MeasureItem(View view)
        {
            Size itemSize = new Size();

            // Reference text height
            int textHeight = ImageListView.Font.Height;

            Size itemPadding = new Size(8, 8);
            itemSize = ImageListView.ThumbnailSize + itemPadding + itemPadding;
            itemSize.Height += textHeight + System.Math.Max(4, textHeight / 3); // textHeight / 3 = vertical space between thumbnail and text

            return itemSize;
        }

        public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
        {
            Size itemPadding = new Size(6, 6);
            bool alternate = (item.Index % 2 == 1);
            bounds = Rectangle.Inflate(bounds, -2, -2);

            // Paint background
            if (ImageListView.Enabled)
            {
                using (Brush bItemBack = new SolidBrush(alternate && ImageListView.View == View.Details ?
                    ImageListView.Colors.AlternateBackColor : ImageListView.Colors.BackColor))
                {
                    g.FillRectangle(bItemBack, bounds);
                }
            }
            else
            {
                using (Brush bItemBack = new SolidBrush(ImageListView.Colors.DisabledBackColor))
                {
                    g.FillRectangle(bItemBack, bounds);
                }
            }

            // Paint background Disabled
            if ((state & ItemState.Disabled) != ItemState.None)
            {
                using (Brush bDisabled = new LinearGradientBrush(bounds, ImageListView.Colors.DisabledColor1, ImageListView.Colors.DisabledColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bDisabled, bounds, (ImageListView.View == View.Details ? 2 : 4));
                }
            }

            // Paint background Selected
            else if ((ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None)) ||
                (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None) && ((state & ItemState.Hovered) != ItemState.None)))
            {
                using (Brush bSelected = new LinearGradientBrush(bounds, ImageListView.Colors.SelectedColor1, ImageListView.Colors.SelectedColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bSelected, bounds, (ImageListView.View == View.Details ? 2 : 4));
                }
            }

            // Paint background unfocused
            else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
            {
                using (Brush bGray64 = new LinearGradientBrush(bounds, ImageListView.Colors.UnFocusedColor1, ImageListView.Colors.UnFocusedColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bGray64, bounds, (ImageListView.View == View.Details ? 2 : 4));
                }
            }

            // Paint background Hovered
            if ((state & ItemState.Hovered) != ItemState.None)
            {
                using (Brush bHovered = new LinearGradientBrush(bounds, ImageListView.Colors.HoverColor1, ImageListView.Colors.HoverColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bHovered, bounds, (ImageListView.View == View.Details ? 2 : 4));
                }
            }

            // Draw the image
            Image img = item.GetCachedImage(CachedImageType.Thumbnail);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize));
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
            Rectangle rt = new Rectangle(bounds.Left + itemPadding.Width, bounds.Top + 2 * itemPadding.Height + ImageListView.ThumbnailSize.Height, ImageListView.ThumbnailSize.Width, szt.Height);
            System.Windows.Forms.TextRenderer.DrawText(g, item.Text, ImageListView.Font, rt, foreColor,
                System.Windows.Forms.TextFormatFlags.EndEllipsis | System.Windows.Forms.TextFormatFlags.HorizontalCenter | System.Windows.Forms.TextFormatFlags.VerticalCenter | System.Windows.Forms.TextFormatFlags.SingleLine);

            // Item border
            if (ImageListView.View != View.Details)
            {
                using (Pen pWhite128 = new Pen(Color.FromArgb(128, ImageListView.Colors.ControlBackColor)))
                {
                    Utility.DrawRoundedRectangle(g, pWhite128, bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3, (ImageListView.View == View.Details ? 2 : 4));
                }
            }
            if (((state & ItemState.Disabled) != ItemState.None))
            {
                using (Pen pHighlight128 = new Pen(ImageListView.Colors.DisabledBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, (ImageListView.View == View.Details ? 2 : 4));
                }
            }
            else if (ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
            {
                using (Pen pHighlight128 = new Pen(ImageListView.Colors.SelectedBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, (ImageListView.View == View.Details ? 2 : 4));
                }
            }
            else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
            {
                using (Pen pGray128 = new Pen(ImageListView.Colors.UnFocusedBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pGray128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, (ImageListView.View == View.Details ? 2 : 4));
                }
            }
            else if (ImageListView.View != View.Details && (state & ItemState.Selected) == ItemState.None)
            {
                using (Pen pGray64 = new Pen(ImageListView.Colors.BorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pGray64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, (ImageListView.View == View.Details ? 2 : 4));
                }
            }

            if (ImageListView.Focused && ((state & ItemState.Hovered) != ItemState.None))
            {
                using (Pen pHighlight64 = new Pen(ImageListView.Colors.HoverBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pHighlight64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, (ImageListView.View == View.Details ? 2 : 4));
                }
            }

            // Focus rectangle
            if (ImageListView.Focused && ((state & ItemState.Focused) != ItemState.None))
            {
                System.Windows.Forms.ControlPaint.DrawFocusRectangle(g, bounds);
            }
        }
    }
}
