using System.Drawing;
using System.Windows.Forms;

namespace Lobster
{
    class DataGridViewDisableButtonColumn : DataGridViewButtonColumn
    {
        public DataGridViewDisableButtonColumn()
        {
            this.CellTemplate = new DataGridViewDisableButtonCell();
        }
    }

    class DataGridViewDisableButtonCell : DataGridViewButtonCell
    {
        private bool enabledValue;
        public bool Enabled
        {
            get { return enabledValue; }
            set { enabledValue = value; }
        }

        public override object Clone()
        {
            DataGridViewDisableButtonCell cell = (DataGridViewDisableButtonCell)base.Clone();
            cell.Enabled = this.Enabled;
            return cell;
        }

        public DataGridViewDisableButtonCell()
        {
            this.enabledValue = true;
        }

        protected override void Paint( Graphics _graphics,
            Rectangle _clipBounds, Rectangle _cellBounds, int _rowIndex,
            DataGridViewElementStates _elementState, object _value,
            object _formattedValue, string _errorText,
            DataGridViewCellStyle _cellStyle,
            DataGridViewAdvancedBorderStyle _advancedBorderStyle,
            DataGridViewPaintParts _paintParts )
        {
            // The button cell is disabled, so paint the border, background, and disabled button for the cell
            if ( !this.enabledValue )
            {
                if ( ( _paintParts & DataGridViewPaintParts.Background ) ==
                    DataGridViewPaintParts.Background )
                {
                    SolidBrush cellBackground =
                        new SolidBrush( _cellStyle.BackColor );
                    _graphics.FillRectangle( cellBackground, _cellBounds );
                    cellBackground.Dispose();
                }

                if ( ( _paintParts & DataGridViewPaintParts.Border ) ==
                    DataGridViewPaintParts.Border )
                {
                    PaintBorder( _graphics, _clipBounds, _cellBounds, _cellStyle, _advancedBorderStyle );
                }

                //Calculate the area in which to draw the button
                Rectangle buttonArea = _cellBounds;
                Rectangle buttonAdjustment =
                    this.BorderWidths( _advancedBorderStyle );
                buttonArea.X += buttonArea.X;
                buttonArea.Y += buttonArea.Y;
                buttonArea.Height -= buttonAdjustment.Height;
                buttonArea.Width -= buttonAdjustment.Width;

                ButtonRenderer.DrawButton( _graphics, buttonArea,
                    System.Windows.Forms.VisualStyles.PushButtonState.Disabled );

                // Draw the disabled button
                if ( this.FormattedValue is string )
                {
                    TextRenderer.DrawText( _graphics,
                        (string)this.FormattedValue,
                        this.DataGridView.Font,
                        buttonArea, SystemColors.GrayText );
                }
            }
            else
            {
                // The button cell is enabled, so let the base class
                // handle the painting
                base.Paint( _graphics, _clipBounds, _cellBounds, _rowIndex,
                    _elementState, _value, _formattedValue, _errorText,
                    _cellStyle, _advancedBorderStyle, _paintParts );
            }
        }
    }
}
