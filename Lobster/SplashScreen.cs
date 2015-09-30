//-----------------------------------------------------------------------
// <copyright file="SplashScreen.cs" company="marshl">
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
namespace Lobster
{
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// The form used to display a logo while the bulk of the program is still loading.
    /// </summary>
    public partial class SplashScreen : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreen"/> class.
        /// </summary>
        public SplashScreen()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Prevents OnPaint from doing anything.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Do nothing here!
        }

        /// <summary>
        /// Replaces the background with the splash image.
        /// </summary>
        /// <param name="e">The event argumeents.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics gfx = e.Graphics;
            gfx.DrawImage(this.BackgroundImage, new Rectangle(0, 0, this.Width, this.Height));
        }
    }
}
