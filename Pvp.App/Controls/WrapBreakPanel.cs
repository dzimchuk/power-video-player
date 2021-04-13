using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pvp.App.Controls
{
    public class WrapBreakPanel : Panel
    {
        public static readonly DependencyProperty BreakProperty =
            DependencyProperty.RegisterAttached("Break", typeof(bool), typeof(WrapBreakPanel),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnBreakChanged)) { AffectsArrange = true, AffectsMeasure = true });

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(WrapBreakPanel),
                new FrameworkPropertyMetadata(default(Orientation)) { AffectsArrange = true, AffectsMeasure = true });

        public static void SetBreak(UIElement element, bool value)
        {
            element.SetValue(BreakProperty, value);
        }

        public static Boolean GetBreak(UIElement element)
        {
            return (bool)element.GetValue(BreakProperty);
        }

        private static void OnBreakChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            obj.SetValue(BreakProperty, args.NewValue);
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return Orientation == Orientation.Horizontal ? MeasureHorizontal(constraint) : MeasureVertical(constraint);
        }

        private Size MeasureHorizontal(Size constraint)
        {
            Size currentLineSize = new Size();
            Size panelSize = new Size();

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(constraint);
                Size desiredSize = element.DesiredSize;

                if (GetBreak(element) ||
                    currentLineSize.Width + desiredSize.Width > constraint.Width)
                {
                    // Switch to a new line (either because the element has requested it
                    // or space has run out).
                    panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
                    panelSize.Height += currentLineSize.Height;
                    currentLineSize = desiredSize;

                    // If the element is too wide to fit using the maximum width of the line,
                    // just give it a separate line.
                    if (desiredSize.Width > constraint.Width)
                    {
                        panelSize.Width = Math.Max(desiredSize.Width, panelSize.Width);
                        panelSize.Height += desiredSize.Height;
                        currentLineSize = new Size();
                    }
                }
                else
                {
                    // Keep adding to the current line.
                    currentLineSize.Width += desiredSize.Width;

                    // Make sure the line is as tall as its tallest element.
                    currentLineSize.Height = Math.Max(desiredSize.Height, currentLineSize.Height);
                }
            }

            // Return the size required to fit all elements.
            // Ordinarily, this is the width of the constraint, and the height
            // is based on the size of the elements.
            // However, if an element is wider than the width given to the panel,
            // the desired width will be the width of that line.
            panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
            panelSize.Height += currentLineSize.Height;
            return panelSize;
        }

        private Size MeasureVertical(Size constraint)
        {
            Size currentColumnSize = new Size();
            Size panelSize = new Size();

            int count = 0;
            foreach (UIElement element in InternalChildren)
            {
                element.Measure(constraint);
                Size desiredSize = element.DesiredSize;

                if (GetBreak(element) ||
                    currentColumnSize.Height + desiredSize.Height > constraint.Height || (count != 0 && count % 18 == 0))
                {
                    // Switch to a new column
                    panelSize.Width += currentColumnSize.Width;
                    panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
                    currentColumnSize = desiredSize;

                    // If the element is too tall to fit using the maximum height of the column,
                    // just give it a separate column.
                    if (desiredSize.Height > constraint.Height)
                    {
                        panelSize.Width += desiredSize.Width;
                        panelSize.Height = Math.Max(desiredSize.Height, panelSize.Height);
                        currentColumnSize = new Size();
                    }
                }
                else
                {
                    // Make sure the column is as wide as its widest element.
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);

                    // Keep adding to the current column.
                    currentColumnSize.Height += desiredSize.Height;
                }

                count++;
            }

            // Return the size required to fit all elements.
            panelSize.Width += currentColumnSize.Width;
            panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
            return panelSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return Orientation == Orientation.Horizontal ? ArrangeHorizontal(arrangeBounds) : ArrangeVertical(arrangeBounds);
        }

        private Size ArrangeHorizontal(Size arrangeBounds)
        {
            int firstInLine = 0;

            Size currentLineSize = new Size();

            double accumulatedHeight = 0;

            UIElementCollection elements = InternalChildren;
            for (int i = 0; i < elements.Count; i++)
            {
                Size desiredSize = elements[i].DesiredSize;

                if (GetBreak(elements[i]) || currentLineSize.Width + desiredSize.Width > arrangeBounds.Width) //need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, i);

                    accumulatedHeight += currentLineSize.Height;
                    currentLineSize = desiredSize;

                    if (desiredSize.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line                    
                    {
                        ArrangeLine(accumulatedHeight, desiredSize.Height, i, ++i);
                        accumulatedHeight += desiredSize.Height;
                        currentLineSize = new Size();
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a line
                {
                    currentLineSize.Width += desiredSize.Width;
                    currentLineSize.Height = Math.Max(desiredSize.Height, currentLineSize.Height);
                }
            }

            if (firstInLine < elements.Count)
                ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, elements.Count);

            return arrangeBounds;
        }

        private void ArrangeLine(double y, double lineHeight, int start, int end)
        {
            double x = 0;
            UIElementCollection children = InternalChildren;
            for (int i = start; i < end; i++)
            {
                UIElement child = children[i];
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineHeight));
                x += child.DesiredSize.Width;
            }
        }

        private Size ArrangeVertical(Size arrangeBounds)
        {
            int firstInColumn = 0;

            Size currentColumnSize = new Size();

            double accumulatedWidth = 0;

            UIElementCollection elements = InternalChildren;
            for (int i = 0; i < elements.Count; i++)
            {
                Size desiredSize = elements[i].DesiredSize;

                if (GetBreak(elements[i]) || currentColumnSize.Height + desiredSize.Height > arrangeBounds.Height || ((i != 0 && i % 18 == 0))) //need to switch to another column
                {
                    ArrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInColumn, i);

                    accumulatedWidth += currentColumnSize.Width;
                    currentColumnSize = desiredSize;

                    if (desiredSize.Height > arrangeBounds.Height) //the element is taller then the constraint - give it a separate column                   
                    {
                        ArrangeColumn(accumulatedWidth, desiredSize.Width, i, ++i);
                        accumulatedWidth += desiredSize.Width;
                        currentColumnSize = new Size();
                    }
                    firstInColumn = i;
                }
                else //continue to accumulate a column
                {
                    currentColumnSize.Height += desiredSize.Height;
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);
                }
            }

            if (firstInColumn < elements.Count)
                ArrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInColumn, elements.Count);

            return arrangeBounds;
        }

        private void ArrangeColumn(double x, double columnWidth, int start, int end)
        {
            double y = 0;
            UIElementCollection children = InternalChildren;
            for (int i = start; i < end; i++)
            {
                UIElement child = children[i];
                child.Arrange(new Rect(x, y, columnWidth, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
            }
        }
    }
}
