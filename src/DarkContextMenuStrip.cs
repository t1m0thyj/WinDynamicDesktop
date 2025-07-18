﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Drawing;

namespace System.Windows.Forms;

using Point = Drawing.Point;
using SystemColors = Drawing.SystemColors;

/// <summary>
/// Provides dark mode rendering capabilities for ToolStrip controls in Windows Forms.
/// </summary>
/// <remarks>
///  <para>
///   This renderer is designed to be used with the ToolStripSystemRenderer to provide dark mode
///   styling while maintaining accessibility features. It inherits from ToolStripRenderer
///   and overrides necessary methods to provide dark-themed rendering.
///   </para>
/// </remarks>
internal class ToolStripSystemDarkModeRenderer : ToolStripRenderer
{
    /// <summary>
    /// Initializes a new instance of the ToolStripSystemDarkModeRenderer class.
    /// </summary>
    public ToolStripSystemDarkModeRenderer()
    {
    }


    /// <summary>
    /// Initializes a new instance of the ToolStripSystemDarkModeRenderer class with the specified default state.
    /// </summary>
    /// <param name="isDefault">true if this is the default renderer; otherwise, false.</param>
    //internal ToolStripSystemDarkModeRenderer(bool isDefault) : base(isDefault)
    //{
    //}

    /// <summary>
    /// Gets dark-appropriate system colors based on the control type.
    /// </summary>
    /// <param name="color">The color to convert to a dark mode equivalent.</param>
    /// <returns>A color suitable for dark mode.</returns>
    private static Color GetDarkModeColor(Color color)
    {
        // Map common system colors to their dark mode equivalents
        if (color == SystemColors.Control)
            return Color.FromArgb(45, 45, 45);
        if (color == SystemColors.ControlLight)
            return Color.FromArgb(60, 60, 60);
        if (color == SystemColors.ControlDark)
            return Color.FromArgb(30, 30, 30);
        if (color == SystemColors.ControlText)
            return Color.FromArgb(240, 240, 240);
        if (color == SystemColors.ButtonFace)
            return Color.FromArgb(45, 45, 45);
        if (color == SystemColors.Highlight)
            return Color.FromArgb(0, 120, 215);
        if (color == SystemColors.HighlightText)
            return Color.White;
        if (color == SystemColors.Window)
            return Color.FromArgb(32, 32, 32);
        if (color == SystemColors.WindowText)
            return Color.FromArgb(240, 240, 240);
        if (color == SystemColors.GrayText)
            return Color.FromArgb(153, 153, 153);
        if (color == SystemColors.InactiveBorder)
            return Color.FromArgb(70, 70, 70);
        if (color == SystemColors.ButtonHighlight)
            return Color.FromArgb(80, 80, 80);
        if (color == SystemColors.ButtonShadow)
            return Color.FromArgb(20, 20, 20);
        if (color == SystemColors.Menu)
            return Color.FromArgb(45, 45, 45);
        if (color == SystemColors.MenuText)
            return Color.FromArgb(240, 240, 240);

        // For any other colors, darken them if they're bright
        if (color.GetBrightness() > 0.5)
        {
            // Create a darker version for light colors
            return ControlPaint.Dark(color, 0.2f);
        }

        return color;
    }

    /// <summary>
    /// Creates a dark mode compatible brush.
    /// </summary>
    /// <param name="color">The system color to convert.</param>
    /// <returns>A brush with the dark mode color.</returns>
    private static SolidBrush GetDarkModeBrush(Color color)
    {
        return new SolidBrush(GetDarkModeColor(color));
    }

    /// <summary>
    /// Creates a dark mode compatible pen.
    /// </summary>
    /// <param name="color">The system color to convert.</param>
    /// <returns>A pen with the dark mode color.</returns>
    private static Pen GetDarkModePen(Color color)
    {
        return new Pen(GetDarkModeColor(color));
    }

    /// <summary>
    /// Returns whether the background should be painted.
    /// </summary>
    /// <param name="toolStrip">The ToolStrip to check.</param>
    /// <returns>true if the background should be painted; otherwise, false.</returns>
    private static bool ShouldPaintBackground(ToolStrip toolStrip)
    {
        if (toolStrip is null)
            return true;

        if (toolStrip.BackgroundImage is not null)
            return false;

        return true;
    }

    /// <summary>
    /// Fills the background with the specified color.
    /// </summary>
    /// <param name="g">The Graphics to draw on.</param>
    /// <param name="bounds">The bounds to fill.</param>
    /// <param name="backColor">The background color.</param>
    private static void FillBackground(Graphics g, Rectangle bounds, Color backColor)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Use a dark mode color
        using Brush brush = GetDarkModeBrush(backColor);
        g.FillRectangle(brush, bounds);
    }

    /// <summary>
    /// Raises the RenderToolStripBackground event.
    /// </summary>
    /// <param name="e">A ToolStripRenderEventArgs that contains the event data.</param>
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        ToolStrip toolStrip = e.ToolStrip;
        Graphics g = e.Graphics;
        Rectangle bounds = e.AffectedBounds;

        if (!ShouldPaintBackground(toolStrip))
            return;

        if (toolStrip is StatusStrip)
        {
            RenderStatusStripBackground(e);
        }
        else
        {
            if (toolStrip.IsDropDown)
            {
                // Dark mode dropdown background
                FillBackground(g, bounds, GetDarkModeColor(SystemColors.Menu));
            }
            else if (toolStrip is MenuStrip)
            {
                // Dark mode menu background
                FillBackground(g, bounds, GetDarkModeColor(SystemColors.Menu));
            }
            else
            {
                // Standard ToolStrip background
                FillBackground(g, bounds, GetDarkModeColor(e.BackColor));
            }
        }
    }

    /// <summary>
    /// Renders the StatusStrip background in dark mode.
    /// </summary>
    /// <param name="e">A ToolStripRenderEventArgs that contains the event data.</param>
    private static void RenderStatusStripBackground(ToolStripRenderEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle bounds = e.AffectedBounds;

        // Dark mode StatusStrip background
        FillBackground(g, bounds, GetDarkModeColor(SystemColors.Control));
    }

    /// <summary>
    /// Raises the RenderToolStripBorder event.
    /// </summary>
    /// <param name="e">A ToolStripRenderEventArgs that contains the event data.</param>
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        ToolStrip toolStrip = e.ToolStrip;
        Graphics g = e.Graphics;
        Rectangle bounds = e.ToolStrip.ClientRectangle;

        if (toolStrip is StatusStrip)
        {
            RenderStatusStripBorder(e);
        }
        else if (toolStrip is ToolStripDropDown)
        {
            ToolStripDropDown? toolStripDropDown = toolStrip as ToolStripDropDown;

            Debug.Assert(toolStripDropDown is not null, $"ToolStripDropDown cannot be null in {nameof(OnRenderToolStripBorder)}.");

            if (toolStripDropDown.DropShadowEnabled)
            {
                bounds.Width -= 1;
                bounds.Height -= 1;

                using Pen borderPen = GetDarkModePen(SystemColors.ControlDark);
                g.DrawRectangle(borderPen, bounds);
            }
            else
            {
                using Pen borderPen = GetDarkModePen(SystemColors.ControlDark);
                g.DrawRectangle(borderPen, bounds);
            }
        }
        else
        {
            // Draw a subtle bottom border for toolstrips
            using Pen borderPen = GetDarkModePen(SystemColors.ControlDark);
            g.DrawLine(borderPen, 0, bounds.Bottom - 1, bounds.Width, bounds.Bottom - 1);
        }
    }

    /// <summary>
    /// Renders the StatusStrip border in dark mode.
    /// </summary>
    /// <param name="e">A ToolStripRenderEventArgs that contains the event data.</param>
    private static void RenderStatusStripBorder(ToolStripRenderEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle bounds = e.ToolStrip.ClientRectangle;

        // Dark mode StatusStrip border (usually top border only)
        using Pen borderPen = GetDarkModePen(SystemColors.ControlDark);
        g.DrawLine(borderPen, 0, 0, bounds.Width, 0);
    }

    /// <summary>
    /// Raises the RenderItemBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

        // For items on dropdowns, adjust the bounds
        if (e.Item.IsOnDropDown)
        {
            bounds.X += 2;
            bounds.Width -= 3;
        }

        if (e.Item.Selected || e.Item.Pressed)
        {
            // Dark mode selection highlight
            using Brush highlightBrush = GetDarkModeBrush(SystemColors.Highlight);
            e.Graphics.FillRectangle(highlightBrush, bounds);
        }
        else
        {
            // Render background image if available
            if (e.Item.BackgroundImage is not null)
            {
                //ControlPaint.DrawBackgroundImage(
                //    e.Graphics,
                //    e.Item.BackgroundImage,
                //    GetDarkModeColor(e.Item.BackColor),
                //    e.Item.BackgroundImageLayout,
                //    e.Item.ContentRectangle,
                //    bounds);
            }
            else if (e.Item.BackColor != Color.Transparent && e.Item.BackColor != Color.Empty)
            {
                // Custom background color (apply dark mode transformation)
                FillBackground(e.Graphics, bounds, e.Item.BackColor);
            }
        }
    }

    /// <summary>
    /// Raises the RenderButtonBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
        bool isPressed;
        bool isSelected;

        if (e.Item is ToolStripButton button)
        {
            isPressed = button.Pressed;
            isSelected = button.Selected || button.Checked;
        }
        else
        {
            isPressed = e.Item.Pressed;
            isSelected = e.Item.Selected;
        }

        if (isPressed || isSelected)
        {
            Color fillColor = isPressed
                ? GetDarkModeColor(SystemColors.ControlDark)
                : GetDarkModeColor(SystemColors.Highlight);

            using Brush fillBrush = new SolidBrush(fillColor);

            e.Graphics.FillRectangle(fillBrush, bounds);
        }
    }

    /// <summary>
    /// Raises the RenderDropDownButtonBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
    {
        // Reuse button background drawing for dropdown buttons
        OnRenderButtonBackground(e);
    }

    /// <summary>
    /// Raises the RenderSplitButtonBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Item is not ToolStripSplitButton splitButton)
        {
            return;
        }

        Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

        // Render the background based on state
        if (splitButton.Selected || splitButton.Pressed)
        {
            Color fillColor = splitButton.Pressed
                ? GetDarkModeColor(SystemColors.ControlDark)
                : GetDarkModeColor(SystemColors.Highlight);

            using Brush fillBrush = new SolidBrush(fillColor);
            e.Graphics.FillRectangle(fillBrush, bounds);
        }

        // Draw the split line
        Rectangle dropDownRect = splitButton.DropDownButtonBounds;
        using Pen linePen = GetDarkModePen(SystemColors.ControlDark);

        e.Graphics.DrawLine(
            linePen,
            dropDownRect.Left - 1,
            dropDownRect.Top + 2,
            dropDownRect.Left - 1,
            dropDownRect.Bottom - 2);
    }

    /// <summary>
    /// Raises the RenderSeparator event.
    /// </summary>
    /// <param name="e">A ToolStripSeparatorRenderEventArgs that contains the event data.</param>
    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Rectangle bounds = e.Item.ContentRectangle;
        bool isVertical = e.Vertical;
        Graphics g = e.Graphics;

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        Color foreColor = GetDarkModeColor(SystemColors.ControlDark);

        bool rightToLeft = e.Item.RightToLeft == RightToLeft.Yes;

        using Pen foreColorPen = new Pen(foreColor);

        if (isVertical)
        {
            // Vertical separator
            using Pen leftPen = rightToLeft
                ? foreColorPen
                : GetDarkModePen(SystemColors.ButtonHighlight);

            using Pen rightPen = rightToLeft
                ? GetDarkModePen(SystemColors.ButtonHighlight)
                : foreColorPen;

            int startX = bounds.Width / 2;
            g.DrawLine(leftPen, startX, bounds.Top, startX, bounds.Bottom);
            startX++;
            g.DrawLine(rightPen, startX, bounds.Top, startX, bounds.Bottom);
        }
        else
        {
            // Horizontal separator
            if (bounds.Width >= 4)
            {
                bounds.Inflate(-2, 0); // Scoot over 2px and start drawing
            }

            int startY = bounds.Height / 2;
            g.DrawLine(foreColorPen, bounds.Left, startY, bounds.Right, startY);
            startY++;
            g.DrawLine(GetDarkModePen(SystemColors.ButtonHighlight), bounds.Left, startY, bounds.Right, startY);
        }
    }

    /// <summary>
    /// Raises the RenderOverflowButtonBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Item is not ToolStripOverflowButton item)
            return;

        Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

        // Render the background based on state
        if (item.Selected || item.Pressed)
        {
            Color fillColor = item.Pressed ?
                GetDarkModeColor(SystemColors.ControlDark) :
                GetDarkModeColor(SystemColors.Highlight);

            using Brush fillBrush = new SolidBrush(fillColor);
            e.Graphics.FillRectangle(fillBrush, bounds);
        }

        // Draw the overflow arrow
        Rectangle arrowRect = item.ContentRectangle;

        Point middle = new Point(
            arrowRect.Left + arrowRect.Width / 2,
            arrowRect.Top + arrowRect.Height / 2);

        // Default to down arrow for overflow buttons
        ArrowDirection arrowDirection = ArrowDirection.Down;

        // Determine actual direction based on dropdown direction
        ToolStripDropDownDirection direction = item.DropDownDirection;

        if (direction is ToolStripDropDownDirection.AboveLeft or ToolStripDropDownDirection.AboveRight)
        {
            arrowDirection = ArrowDirection.Up;
        }
        else if (direction == ToolStripDropDownDirection.Left)
        {
            arrowDirection = ArrowDirection.Left;
        }
        else if (direction == ToolStripDropDownDirection.Right)
        {
            arrowDirection = ArrowDirection.Right;
        }

        // else default to ArrowDirection.Down

        // Set arrow color based on state
        Color arrowColor = GetDarkModeColor(SystemColors.ControlText);

        if (item.Pressed || item.Selected)
        {
            arrowColor = GetDarkModeColor(SystemColors.HighlightText);
        }

        // Define arrow polygon based on direction
        Point[] arrow = arrowDirection switch
        {
            ArrowDirection.Up =>
                [
                    new Point(middle.X - 2, middle.Y + 1),
                    new Point(middle.X + 3, middle.Y + 1),
                    new Point(middle.X, middle.Y - 2)
                ],
            ArrowDirection.Left =>
                [
                    new Point(middle.X + 2, middle.Y - 4),
                    new Point(middle.X + 2, middle.Y + 4),
                    new Point(middle.X - 2, middle.Y)
                ],
            ArrowDirection.Right =>
                [
                    new Point(middle.X - 2, middle.Y - 4),
                    new Point(middle.X - 2, middle.Y + 4),
                    new Point(middle.X + 2, middle.Y)
                ],
            _ =>
                [
                    new Point(middle.X - 2, middle.Y - 1),
                    new Point(middle.X + 3, middle.Y - 1),
                    new Point(middle.X, middle.Y + 2)
                ],
        };

        // Draw the arrow
        using Brush arrowBrush = new SolidBrush(arrowColor);
        e.Graphics.FillPolygon(arrowBrush, arrow);
    }

    /// <summary>
    /// Raises the RenderGrip event.
    /// </summary>
    /// <param name="e">A ToolStripGripRenderEventArgs that contains the event data.</param>
    protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.GripBounds.Width <= 0 || e.GripBounds.Height <= 0)
            return;

        // Use dark mode colors for the grip dots
        Color darkColor = GetDarkModeColor(SystemColors.ControlDark);
        Color lightColor = GetDarkModeColor(SystemColors.ControlLight);

        ToolStrip toolStrip = e.ToolStrip;
        Graphics g = e.Graphics;
        Rectangle bounds = e.GripBounds;

        // Draw grip dots
        if (toolStrip.Orientation == Orientation.Horizontal)
        {
            // Draw vertical grip
            int y = bounds.Top + 2;

            while (y < bounds.Bottom - 3)
            {
                g.FillRectangle(new SolidBrush(darkColor), bounds.Left + 2, y, 1, 1);
                g.FillRectangle(new SolidBrush(lightColor), bounds.Left + 3, y + 1, 1, 1);
                y += 3;
            }
        }
        else
        {
            // Draw horizontal grip
            int x = bounds.Left + 2;

            while (x < bounds.Right - 3)
            {
                g.FillRectangle(new SolidBrush(darkColor), x, bounds.Top + 2, 1, 1);
                g.FillRectangle(new SolidBrush(lightColor), x + 1, bounds.Top + 3, 1, 1);
                x += 3;
            }
        }
    }

    /// <summary>
    /// Raises the RenderArrow event.
    /// </summary>
    /// <param name="e">A ToolStripArrowRenderEventArgs that contains the event data.</param>
    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        Debug.Assert(e.Item is not null, "The ToolStripItem should not be null on rendering the Arrow.");

        Color arrowColor = GetDarkModeColor(e.ArrowColor);

        // Use white arrow for selected/highlighted items
        if (e.Item.Selected || e.Item.Pressed)
        {
            arrowColor = GetDarkModeColor(SystemColors.HighlightText);
        }

        // Draw the arrow with the appropriate color
        Rectangle rect = e.ArrowRectangle;
        Graphics g = e.Graphics;

        Point middle = new Point(
            rect.Left + rect.Width / 2,
            rect.Top + rect.Height / 2);

        // Define the arrow polygon based on direction
        Point[] arrow;

        arrow = e.Direction switch
        {
            ArrowDirection.Up =>
                [
                    new Point(middle.X - 2, middle.Y + 1),
                    new Point(middle.X + 3, middle.Y + 1),
                    new Point(middle.X, middle.Y - 2)
                ],
            ArrowDirection.Left =>
                [
                    new Point(middle.X + 2, middle.Y - 4),
                    new Point(middle.X + 2, middle.Y + 4),
                    new Point(middle.X - 2, middle.Y)
                ],
            ArrowDirection.Right =>
                [
                    new Point(middle.X - 2, middle.Y - 4),
                    new Point(middle.X - 2, middle.Y + 4),
                    new Point(middle.X + 2, middle.Y)
                ],
            _ =>
                [
                    new Point(middle.X - 2, middle.Y - 1),
                    new Point(middle.X + 3, middle.Y - 1),
                    new Point(middle.X, middle.Y + 2)
                ]
        };

        using Brush brush = new SolidBrush(arrowColor);
        g.FillPolygon(brush, arrow);
    }

    /// <summary>
    /// Raises the RenderImageMargin event.
    /// </summary>
    /// <param name="e">A ToolStripRenderEventArgs that contains the event data.</param>
    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        // Fill the image margin with a slightly different color than the background
        Color marginColor = GetDarkModeColor(SystemColors.ControlLight);
        using Brush brush = new SolidBrush(marginColor);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    /// <summary>
    /// Raises the RenderItemText event.
    /// </summary>
    /// <param name="e">A ToolStripItemTextRenderEventArgs that contains the event data.</param>
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        // Set text color based on selection state
        Color textColor;

        if (e.Item.Selected || e.Item.Pressed)
        {
            textColor = GetDarkModeColor(SystemColors.HighlightText);
        }
        else if (!e.Item.Enabled)
        {
            textColor = GetDarkModeColor(SystemColors.GrayText);
        }
        else
        {
            // Use the original text color but make sure it's dark mode compatible
            textColor = GetDarkModeColor(e.TextColor);
        }

        // Draw the text
        TextRenderer.DrawText(
            e.Graphics,
            e.Text,
            e.TextFont,
            e.TextRectangle,
            textColor,
            e.TextFormat);
    }

    /// <summary>
    /// Raises the RenderItemImage event.
    /// </summary>
    /// <param name="e">A ToolStripItemImageRenderEventArgs that contains the event data.</param>
    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Image is null)
            return;

        // Draw the image normally, no need to adjust for dark mode as images
        // should already be designed for the appropriate context
        if (!e.Item.Enabled)
        {
            // For disabled items, draw a grayscale version
            ControlPaint.DrawImageDisabled(
                e.Graphics,
                e.Image,
                e.ImageRectangle.X,
                e.ImageRectangle.Y,
                GetDarkModeColor(SystemColors.Control));
        }
        else
        {
            e.Graphics.DrawImage(e.Image, e.ImageRectangle);
        }
    }

    /// <summary>
    /// Raises the RenderLabelBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
    {
        // Use default item background rendering
        OnRenderItemBackground(e);
    }

    /// <summary>
    /// Raises the RenderToolStripStatusLabelBackground event.
    /// </summary>
    /// <param name="e">A ToolStripItemRenderEventArgs that contains the event data.</param>
    protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Rectangle bounds = e.AffectedBounds;
        Graphics g = e.Graphics;

        // Draw a dark mode sizing grip
        Color darkColor = GetDarkModeColor(SystemColors.ControlDark);
        Color lightColor = GetDarkModeColor(SystemColors.ControlLight);

        // Start at the bottom-right corner and move up and left
        int x = bounds.Right - 3;
        int y = bounds.Bottom - 3;

        for (int i = 0; i < 3; i++)
        {
            int tempX = x;
            int tempY = y;

            for (int j = 0; j <= i; j++)
            {
                g.FillRectangle(new SolidBrush(darkColor), tempX, tempY, 2, 2);
                g.FillRectangle(new SolidBrush(lightColor), tempX - 1, tempY - 1, 1, 1);

                tempX -= 4;
                tempY += 4;
            }

            x -= 4;
            y -= 4;
        }
    }
}
