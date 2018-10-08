//-----------------------------------------------------------------------
// <copyright file="BooleanAndConverter.cs" company="marshl">
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
    /// Combines any number of input values in a WPF binding to a single output value that is the AND of all the input booleans
    /// </summary>
    public class BooleanAndConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts the given values to a single boolean
        /// </summary>
        /// <param name="values">The values to convert</param>
        /// <param name="targetType">The type to target</param>
        /// <param name="parameter">The parameter being bound to</param>
        /// <param name="culture">The culute info</param>
        /// <returns>True or false</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Empty function to convert the values back (unused)
        /// </summary>
        /// <param name="value">The input value</param>
        /// <param name="targetTypes">The output target types</param>
        /// <param name="parameter">The parameter</param>
        /// <param name="culture">The culture info</param>
        /// <returns>The output values</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
