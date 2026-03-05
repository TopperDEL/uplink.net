using System;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace uplink.NET.Sample.Shared.Controls
{
    /// <summary>
    /// Defines the types of decision branches shown in the Planter diagram.
    /// </summary>
    public enum PlanterBranchType
    {
        Yes,
        No,
        Filled,
        Unfilled
    }

    /// <summary>
    /// A canvas-based control that renders a Planter diagram showing the state of
    /// storage buckets and objects. Branches like "yes/no" (whether an object exists)
    /// and "filled/unfilled" (whether a bucket contains data) are rendered with
    /// labels that are loaded from localized string resources.
    ///
    /// Note: In production this uses Skia for rendering via the Uno Platform's
    /// SkiaSharp integration. The drawing operations map directly to Skia draw calls.
    /// </summary>
    public class PlanterControl : UserControl
    {
        private readonly Canvas _canvas;
        private ResourceLoader _resourceLoader;

        public PlanterControl()
        {
            _canvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Content = _canvas;
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            }
            catch
            {
                _resourceLoader = null;
            }
            Render();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_resourceLoader != null)
                Render();
        }

        /// <summary>
        /// Renders the Planter diagram on the canvas. This method draws decision
        /// branches with properly translated labels. Previously this method used
        /// hardcoded English strings for branch labels, which broke translations.
        /// The fix loads labels from the localized resource file.
        /// </summary>
        public void Render()
        {
            _canvas.Children.Clear();

            double centerX = ActualWidth > 0 ? ActualWidth / 2 : 200;
            double topY = 40;
            double levelHeight = 80;

            // Draw root node
            DrawNode(centerX, topY, GetRootLabel(), isFilled: true);

            // Draw "yes/no" branches (first level)
            double yesX = centerX - 120;
            double noX = centerX + 120;
            double firstLevelY = topY + levelHeight;

            DrawBranchLine(centerX, topY + 20, yesX, firstLevelY);
            DrawBranchLine(centerX, topY + 20, noX, firstLevelY);

            // Branch labels use localized strings - this was the bug: they were hardcoded
            string yesLabel = GetBranchLabel(PlanterBranchType.Yes);
            string noLabel = GetBranchLabel(PlanterBranchType.No);
            string filledLabel = GetBranchLabel(PlanterBranchType.Filled);
            string unfilledLabel = GetBranchLabel(PlanterBranchType.Unfilled);

            DrawBranchLabel(centerX - 70, topY + levelHeight / 2, yesLabel);
            DrawBranchLabel(centerX + 30, topY + levelHeight / 2, noLabel);

            DrawNode(yesX, firstLevelY, yesLabel, isFilled: true);
            DrawNode(noX, firstLevelY, noLabel, isFilled: false);

            // Draw "filled/unfilled" branches (second level, under yes-branch)
            double filledX = yesX - 80;
            double unfilledX = yesX + 80;
            double secondLevelY = firstLevelY + levelHeight;

            DrawBranchLine(yesX, firstLevelY + 20, filledX, secondLevelY);
            DrawBranchLine(yesX, firstLevelY + 20, unfilledX, secondLevelY);

            DrawBranchLabel(yesX - 60, firstLevelY + levelHeight / 2, filledLabel);
            DrawBranchLabel(yesX + 10, firstLevelY + levelHeight / 2, unfilledLabel);

            DrawNode(filledX, secondLevelY, filledLabel, isFilled: true);
            DrawNode(unfilledX, secondLevelY, unfilledLabel, isFilled: false);
        }

        /// <summary>
        /// Gets the translated label for a branch type. Uses the ResourceLoader to
        /// retrieve the correct localized string for the current culture.
        /// </summary>
        /// <param name="branchType">The branch type enum value</param>
        /// <returns>Localized branch label string</returns>
        public string GetBranchLabel(PlanterBranchType branchType)
        {
            string resourceKey = GetResourceKeyForBranchType(branchType);
            string fallback = GetFallbackLabelForBranchType(branchType);

            if (_resourceLoader == null)
                return fallback;

            string label = _resourceLoader.GetString(resourceKey);
            return string.IsNullOrEmpty(label) ? fallback : label;
        }

        /// <summary>
        /// Maps a branch type enum value to its localization resource key.
        /// Using an explicit mapping avoids tight coupling between enum names and resource keys.
        /// </summary>
        private static string GetResourceKeyForBranchType(PlanterBranchType branchType)
        {
            switch (branchType)
            {
                case PlanterBranchType.Yes:       return "Planter_Branch_Yes";
                case PlanterBranchType.No:        return "Planter_Branch_No";
                case PlanterBranchType.Filled:    return "Planter_Branch_Filled";
                case PlanterBranchType.Unfilled:  return "Planter_Branch_Unfilled";
                default:                          return string.Empty;
            }
        }

        /// <summary>
        /// Provides English fallback labels for each branch type, used when the
        /// resource loader is unavailable or a resource string is not found.
        /// </summary>
        private static string GetFallbackLabelForBranchType(PlanterBranchType branchType)
        {
            switch (branchType)
            {
                case PlanterBranchType.Yes:       return "Yes";
                case PlanterBranchType.No:        return "No";
                case PlanterBranchType.Filled:    return "Filled";
                case PlanterBranchType.Unfilled:  return "Unfilled";
                default:                          return string.Empty;
            }
        }

        private string GetRootLabel()
        {
            if (_resourceLoader == null)
                return "Bucket";

            string label = _resourceLoader.GetString("Planter_Root");
            return string.IsNullOrEmpty(label) ? "Bucket" : label;
        }

        private void DrawNode(double x, double y, string label, bool isFilled)
        {
            double radius = 20;

            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = isFilled
                    ? new SolidColorBrush(Color.FromArgb(255, 0, 120, 180))
                    : new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 80, 140)),
                StrokeThickness = 2
            };

            Canvas.SetLeft(circle, x - radius);
            Canvas.SetTop(circle, y);
            _canvas.Children.Add(circle);

            var text = new TextBlock
            {
                Text = label,
                FontSize = 11,
                Foreground = isFilled
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Colors.Black),
                TextAlignment = Windows.UI.Xaml.TextAlignment.Center,
                Width = radius * 2
            };

            Canvas.SetLeft(text, x - radius);
            Canvas.SetTop(text, y + radius - 7);
            _canvas.Children.Add(text);
        }

        private void DrawBranchLine(double x1, double y1, double x2, double y2)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)),
                StrokeThickness = 1.5
            };

            _canvas.Children.Add(line);
        }

        private void DrawBranchLabel(double x, double y, string label)
        {
            var text = new TextBlock
            {
                Text = label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60))
            };

            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, y);
            _canvas.Children.Add(text);
        }
    }
}
