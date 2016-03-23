//-----------------------------------------------------------------------
// <copyright file="InverseBooleanConverter.cs" company="marshl">
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
//      Sam reeled, clutching at the stone. He felt as
//      if the whole dark world was turning upside down.
//
//      [ _The Lord of the Rings_, V/x: "The Choices of Master Samwise"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Windows.Data;

    /// <summary>
    /// Converted for converting from a boolean value to its inverse value (true => false, false => true)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts the given boolean into its inverse value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to convert into.</param>
        /// <param name="parameter">The parameter that does something.</param>
        /// <param name="culture">The culture info to convert with.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }

        /// <summary>
        /// Converts the value back to its original value (unimplemented).
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The parameter that does something.</param>
        /// <param name="culture">The cutlure info to convert with.</param>
        /// <returns>The converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
