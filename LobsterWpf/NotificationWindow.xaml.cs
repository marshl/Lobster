using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private static List<bool> activeWindowList;

        private int windowIndex;

        public NotificationWindow()
        {
            InitializeComponent();

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

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                Matrix transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                Point corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                this.Left = corner.X;
                this.Top = corner.Y - this.ActualHeight * (this.windowIndex + 1);
                //ScaleTransform
                //UIElement

                TimeSpan slideDuration = new TimeSpan(0, 0, 0, 0, 250);
                TimeSpan pauseDuration = new TimeSpan(0, 0, 0, 1, 0);
                TimeSpan fadeDuration = new TimeSpan(0, 0, 0, 2, 0);

                var sb = new Storyboard { Duration = new Duration(slideDuration + fadeDuration + pauseDuration) };

                //var aniWidth = new DoubleAnimationUsingKeyFrames();
                var aniHeight = new DoubleAnimationUsingKeyFrames();
                var fadeAnimation = new DoubleAnimationUsingKeyFrames();

                //aniWidth.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
                aniHeight.Duration = new Duration(slideDuration);
                fadeAnimation.Duration = new Duration(fadeDuration);

                aniHeight.KeyFrames.Add(new EasingDoubleKeyFrame(corner.X, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                aniHeight.KeyFrames.Add(new EasingDoubleKeyFrame(corner.X - this.ActualWidth, KeyTime.FromTimeSpan(slideDuration)));
                fadeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(slideDuration + pauseDuration)));
                fadeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(fadeDuration)));

                //aniWidth.KeyFrames.Add(new EasingDoubleKeyFrame(target.ActualWidth, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                //aniWidth.KeyFrames.Add(new EasingDoubleKeyFrame(newWidth, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 200))));

                //Storyboard.SetTarget(aniWidth, target);
                //Storyboard.SetTargetProperty(aniWidth, new PropertyPath(Window.WidthProperty));

                Storyboard.SetTarget(aniHeight, this);
                Storyboard.SetTargetProperty(aniHeight, new PropertyPath(Window.LeftProperty));

                Storyboard.SetTarget(fadeAnimation, this);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

                //sb.Children.Add(aniWidth);
                sb.Children.Add(aniHeight);
                sb.Children.Add(fadeAnimation);

                sb.Completed += Storyboard_Completed;

                sb.Begin();

            }));
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            activeWindowList[windowIndex] = false;
            this.Close();
        }
    }
}
