//-----------------------------------------------------------------------
// <copyright file="NotificationWindow.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//      'What was that?' asked Beregond. 'You also felt something?'
//      'Yes,' muttered Pippin. 'It is the sign of our fall, and the shadow of
//       doom, a Fell Rider of the air.'
//
//      [ _The Lord of the Rings_, V/i: "Minas Tirith"]
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private static List<bool> activeWindowList;

        private int windowIndex;

        public string Message { get; set; }

        public ImageSource ImageSource { get; set; }

        public NotificationWindow(string message, bool result)
        {
            InitializeComponent();

            this.Message = message;
            this.DataContext = this;

            this.ImageSource = (ImageSource)(result ? Application.Current.FindResource("SuccessImageSource") : Application.Current.FindResource("WarningImageSource"));

            if (activeWindowList == null)
            {
                activeWindowList = new List<bool>();
            }

            for (int i = 0; ; ++i)
            {
                if (activeWindowList.Count <= i)
                {
                    activeWindowList.Add(true);
                    this.windowIndex = i;
                    break;
                }

                if (!activeWindowList[i])
                {
                    activeWindowList[i] = true;
                    this.windowIndex = i;
                    break;
                }
            }


            System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = workingArea.Right - this.ActualWidth;
            this.Top = workingArea.Top - this.ActualHeight * (this.windowIndex + 1);

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                Matrix transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                Point corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));
                this.Left = corner.X;
                this.Top = corner.Y - this.ActualHeight * (this.windowIndex + 1);

                TimeSpan slideDuration = new TimeSpan(0, 0, 0, 0, 250);
                TimeSpan pauseDuration = new TimeSpan(0, 0, 0, 1, 0);
                TimeSpan fadeDuration = new TimeSpan(0, 0, 0, 2, 0);

                var sb = new Storyboard { Duration = new Duration(slideDuration + fadeDuration + pauseDuration) };

                var heightAnimation = new DoubleAnimationUsingKeyFrames();
                var fadeAnimation = new DoubleAnimationUsingKeyFrames();

                heightAnimation.Duration = new Duration(slideDuration);
                fadeAnimation.Duration = new Duration(fadeDuration);

                heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(corner.X, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(corner.X - this.ActualWidth, KeyTime.FromTimeSpan(slideDuration)));
                fadeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(slideDuration + pauseDuration)));
                fadeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(fadeDuration)));

                Storyboard.SetTarget(heightAnimation, this);
                Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(Window.LeftProperty));

                Storyboard.SetTarget(fadeAnimation, this);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

                sb.Children.Add(heightAnimation);
                sb.Children.Add(fadeAnimation);

                sb.Completed += Storyboard_Completed;

                sb.Begin();

            }));
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            NotificationWindow.activeWindowList[windowIndex] = false;
            this.Close();
        }
    }
}
