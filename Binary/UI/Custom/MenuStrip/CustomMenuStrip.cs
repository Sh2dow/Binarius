using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public partial class CustomMenuStrip : MenuStrip
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of System.Windows.Forms.CustomMenuStrip.
        /// </summary>
        public CustomMenuStrip()
        {
            InitializeComponent();

            this.RenderMode = ToolStripRenderMode.Professional;
            this.Renderer = new ToolStripProfessionalRenderer(new CustomMenuStripColorTable());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the ForeColor of the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuStripForeColor")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuStripForeColor
        {
            get { return Properties.Settings.Default.MenuStripForeColor; }
            set
            {
                Properties.Settings.Default.MenuStripForeColor = value;
                this.ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the start color of the gradient used in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuStripGradientBegin")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuStripGradientBegin
        {
            get { return Properties.Settings.Default.MenuStripGradientBegin; }
            set { Properties.Settings.Default.MenuStripGradientBegin = value; }
        }

        /// <summary>
        /// Gets or sets the end color of the gradient used in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuStripGradientEnd")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuStripGradientEnd
        {
            get { return Properties.Settings.Default.MenuStripGradientEnd; }
            set { Properties.Settings.Default.MenuStripGradientEnd = value; }
        }

        /// <summary>
        /// Gets or sets the start color of the gradient used when the top-level menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemPressedGradientBegin")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemPressedGradientBegin
        {
            get { return Properties.Settings.Default.MenuStripGradientEnd; }
            set { Properties.Settings.Default.MenuStripGradientEnd = value; }
        }

        /// <summary>
        /// Gets or sets the middle color of the gradient used when the top-level menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemPressedGradientMiddle")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemPressedGradientMiddle
        {
            get { return Properties.Settings.Default.MenuStripGradientEnd; }
            set { Properties.Settings.Default.MenuStripGradientEnd = value; }
        }

        /// <summary>
        /// Gets or sets the end color of the gradient used when the top-level menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemPressedGradientEnd")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemPressedGradientEnd
        {
            get { return Properties.Settings.Default.MenuStripGradientEnd; }
            set { Properties.Settings.Default.MenuStripGradientEnd = value; }
        }

        /// <summary>
        /// Gets or sets the end color of the gradient used when the menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemSelectedGradientBegin")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemSelectedGradientBegin
        {
            get { return Properties.Settings.Default.MenuItemSelectedGradientBegin; }
            set { Properties.Settings.Default.MenuItemSelectedGradientBegin = value; }
        }

        /// <summary>
        /// Gets or sets the end color of the gradient used when the menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemSelectedGradientEnd")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemSelectedGradientEnd
        {
            get { return Properties.Settings.Default.MenuItemSelectedGradientEnd; }
            set { Properties.Settings.Default.MenuItemSelectedGradientEnd = value; }
        }

        /// <summary>
        /// Gets or sets the color used when the menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemSelected")]
        [Description("The color used when the menu item is selected in the System.Windows.Forms.MenuStrip control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemSelected
        {
            get { return Properties.Settings.Default.MenuItemSelected; }
            set { Properties.Settings.Default.MenuItemSelected = value; }
        }

        /// <summary>
        /// Gets or sets the color used when the menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuBorder")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuBorder
        {
            get { return Properties.Settings.Default.MenuBorder; }
            set { Properties.Settings.Default.MenuBorder = value; }
        }

        /// <summary>
        /// Gets or sets the color used when the menu item is selected in the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("MenuItemBorder")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color MenuItemBorder
        {
            get { return Properties.Settings.Default.MenuItemBorder; }
            set { Properties.Settings.Default.MenuItemBorder = value; }
        }

        /// <summary>
        /// Gets or sets the starting color of the gradient used in the image margin of the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("ImageMarginGradientBegin")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ImageMarginGradientBegin
        {
            get { return Properties.Settings.Default.ImageMarginGradientBegin; }
            set { Properties.Settings.Default.ImageMarginGradientBegin = value; }
        }

        /// <summary>
        /// Gets or sets the middle color of the gradient used in the image margin of the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("ImageMarginGradientMiddle")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ImageMarginGradientMiddle
        {
            get { return Properties.Settings.Default.ImageMarginGradientMiddle; }
            set { Properties.Settings.Default.ImageMarginGradientMiddle = value; }
        }

        /// <summary>
        /// Gets or sets the ending color of the gradient used in the image margin of the System.Windows.Forms.MenuStrip control.
        /// </summary>
        [Category("Style")]
        [DisplayName("ImageMarginGradientEnd")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ImageMarginGradientEnd
        {
            get { return Properties.Settings.Default.ImageMarginGradientEnd; }
            set { Properties.Settings.Default.ImageMarginGradientEnd = value; }
        }
        #endregion
    }
}
