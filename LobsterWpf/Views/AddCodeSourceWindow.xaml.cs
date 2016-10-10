//-----------------------------------------------------------------------
// <copyright file="AddCodeSourceWindow.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
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
//      "But great though his lore may be, it must have a source."
//        - Gandalf the Grey
//      "I have not brought you hither to be instructed by you, but to give you a choice."
//          - Saruman the Wise
//
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//-----------------------------------------------------------------------
namespace LobsterWpf.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for AddCodeSourceWindow.xaml
    /// </summary>
    public partial class AddCodeSourceWindow : Window
    {
        public enum Selection
        {
            AddPreparedCodeSource,
            PrepareNewCodeSource
        }

        public Selection? UserSelection { get; private set; }

        public AddCodeSourceWindow()
        {
            InitializeComponent();
        }

        private void addPreparedCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelection = Selection.AddPreparedCodeSource;
            this.DialogResult = true;
            this.Close();
        }

        private void prepareNewCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelection = Selection.PrepareNewCodeSource;
            this.DialogResult = true;
            this.Close();
        }
    }
}
