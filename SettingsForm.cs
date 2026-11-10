using System;
using System.Drawing;
using System.Windows.Forms;

namespace RevitShutteringAddin
{
	public class SettingsForm : Form
	{
		private ShutteringSettings _settings;

		private CheckBox chkSlabs;

		private CheckBox chkBeams;

		private CheckBox chkColumns;

		private CheckBox chkWalls;

		private NumericUpDown numPanelThk;

		private CheckBox chkGenerateSupports;

		private NumericUpDown numPrimaryJoist;

		private NumericUpDown numSecondaryJoist;

		private NumericUpDown numPropGridX;

		private NumericUpDown numPropGridY;

		private NumericUpDown numWalerSpacing;

		private RadioButton rdoAutoSlabBelow;

		private RadioButton rdoFixedHeight;

		private NumericUpDown numFixedHeight;

		private CheckBox chkSkipGroundProps;

		private CheckBox chkTieRods;

		private CheckBox chkHandrails;

		private ComboBox cmbPreset;

		private CheckBox chkExportCsv;

		// --- Component Category Modeling Checkboxes ---
		private CheckBox chkModelPlywood;
		private CheckBox chkModelPrimaryJoists;
		private CheckBox chkModelSecondaryJoists;
		private CheckBox chkModelWalers;
		private CheckBox chkModelProps;
		private CheckBox chkSeparateCategories;

		private Button btnOk;

		private Button btnCancel;

		public SettingsForm(ShutteringSettings settings)
		{
			_settings = settings;
			InitializeComponent();
			LoadSettingsToUI();
		}

		private void InitializeComponent()
		{
			this.Text = "BIM TOOLS - Advanced DOKA / Universal Formwork Engine";
			base.Size = new System.Drawing.Size(540, 830);
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular);
			this.BackColor = System.Drawing.Color.FromArgb(245, 246, 248);
			int num = 15;
			int num2 = 495;
			System.Windows.Forms.GroupBox groupBox = new System.Windows.Forms.GroupBox
			{
				Text = "Formwork System & Categories to Process",
				Location = new System.Drawing.Point(num, 10),
				Size = new System.Drawing.Size(num2, 105)
			};
			System.Windows.Forms.Label value = new System.Windows.Forms.Label
			{
				Text = "System Style Preset:",
				Location = new System.Drawing.Point(15, 25),
				AutoSize = true
			};
			this.cmbPreset = new System.Windows.Forms.ComboBox
			{
				Location = new System.Drawing.Point(180, 22),
				Width = 295,
				DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
			};
			this.cmbPreset.Items.Add("DOKA Official (Yellow / Blue / Steel)");
			this.cmbPreset.Items.Add("PERI Signature (Red / Yellow / Silver)");
			this.cmbPreset.Items.Add("Universal Standard Shuttering");
			this.cmbPreset.SelectedIndex = 0;
			this.chkSlabs = new System.Windows.Forms.CheckBox
			{
				Text = "Slabs (Bottom & Sides)",
				Location = new System.Drawing.Point(15, 55),
				AutoSize = true,
				Checked = true
			};
			this.chkBeams = new System.Windows.Forms.CheckBox
			{
				Text = "Beams (Bottom & Sides)",
				Location = new System.Drawing.Point(250, 55),
				AutoSize = true,
				Checked = true
			};
			this.chkColumns = new System.Windows.Forms.CheckBox
			{
				Text = "Columns (Sides)",
				Location = new System.Drawing.Point(15, 78),
				AutoSize = true,
				Checked = true
			};
			this.chkWalls = new System.Windows.Forms.CheckBox
			{
				Text = "Walls / Core Walls (Sides)",
				Location = new System.Drawing.Point(250, 78),
				AutoSize = true,
				Checked = true
			};
			groupBox.Controls.Add(value);
			groupBox.Controls.Add(this.cmbPreset);
			groupBox.Controls.Add(this.chkSlabs);
			groupBox.Controls.Add(this.chkBeams);
			groupBox.Controls.Add(this.chkColumns);
			groupBox.Controls.Add(this.chkWalls);
			System.Windows.Forms.GroupBox groupBox2 = new System.Windows.Forms.GroupBox
			{
				Text = "Formwork Panel Settings",
				Location = new System.Drawing.Point(num, 120),
				Size = new System.Drawing.Size(num2, 60)
			};
			System.Windows.Forms.Label value2 = new System.Windows.Forms.Label
			{
				Text = "Formwork Panel Thickness (mm):",
				Location = new System.Drawing.Point(15, 25),
				AutoSize = true
			};
			this.numPanelThk = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(250, 22),
				Width = 90,
				Minimum = 5m,
				Maximum = 100m,
				Value = 20m
			};
			groupBox2.Controls.Add(value2);
			groupBox2.Controls.Add(this.numPanelThk);
			System.Windows.Forms.GroupBox groupBox3 = new System.Windows.Forms.GroupBox
			{
				Text = "3D Support Systems & DOKA Component Hardware",
				Location = new System.Drawing.Point(num, 185),
				Size = new System.Drawing.Size(num2, 220)
			};
			this.chkGenerateSupports = new System.Windows.Forms.CheckBox
			{
				Text = "Generate 3D Support Beams, Walers & Scaffolding Props",
				Location = new System.Drawing.Point(15, 22),
				AutoSize = true,
				Checked = true
			};
			System.Windows.Forms.Label value3 = new System.Windows.Forms.Label
			{
				Text = "Primary H20 Joist Spacing (mm):",
				Location = new System.Drawing.Point(15, 50),
				AutoSize = true
			};
			this.numPrimaryJoist = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(250, 47),
				Width = 90,
				Minimum = 100m,
				Maximum = 3000m,
				Value = 500m,
				Increment = 50m
			};
			System.Windows.Forms.Label value4 = new System.Windows.Forms.Label
			{
				Text = "Secondary Cross Joist Spacing (mm):",
				Location = new System.Drawing.Point(15, 80),
				AutoSize = true
			};
			this.numSecondaryJoist = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(250, 77),
				Width = 90,
				Minimum = 100m,
				Maximum = 3000m,
				Value = 1000m,
				Increment = 50m
			};
			System.Windows.Forms.Label value5 = new System.Windows.Forms.Label
			{
				Text = "Scaffold Prop Grid X x Y (mm):",
				Location = new System.Drawing.Point(15, 110),
				AutoSize = true
			};
			this.numPropGridX = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(250, 107),
				Width = 70,
				Minimum = 300m,
				Maximum = 4000m,
				Value = 1200m,
				Increment = 100m
			};
			System.Windows.Forms.Label value6 = new System.Windows.Forms.Label
			{
				Text = "x",
				Location = new System.Drawing.Point(325, 110),
				AutoSize = true
			};
			this.numPropGridY = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(340, 107),
				Width = 70,
				Minimum = 300m,
				Maximum = 4000m,
				Value = 1200m,
				Increment = 100m
			};
			System.Windows.Forms.Label value7 = new System.Windows.Forms.Label
			{
				Text = "Waler Vertical Spacing (mm):",
				Location = new System.Drawing.Point(15, 140),
				AutoSize = true
			};
			this.numWalerSpacing = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(250, 137),
				Width = 90,
				Minimum = 200m,
				Maximum = 2000m,
				Value = 600m,
				Increment = 50m
			};
			this.chkTieRods = new System.Windows.Forms.CheckBox
			{
				Text = "Dywidag 15mm Tie Rods & DOKA Wing-Nut Plates",
				Location = new System.Drawing.Point(15, 168),
				AutoSize = true,
				Checked = true
			};
			this.chkHandrails = new System.Windows.Forms.CheckBox
			{
				Text = "DOKA Perimeter Safety Protection Posts",
				Location = new System.Drawing.Point(270, 168),
				AutoSize = true,
				Checked = true
			};
			groupBox3.Controls.Add(this.chkGenerateSupports);
			groupBox3.Controls.Add(value3);
			groupBox3.Controls.Add(this.numPrimaryJoist);
			groupBox3.Controls.Add(value4);
			groupBox3.Controls.Add(this.numSecondaryJoist);
			groupBox3.Controls.Add(value5);
			groupBox3.Controls.Add(this.numPropGridX);
			groupBox3.Controls.Add(value6);
			groupBox3.Controls.Add(this.numPropGridY);
			groupBox3.Controls.Add(value7);
			groupBox3.Controls.Add(this.numWalerSpacing);
			groupBox3.Controls.Add(this.chkTieRods);
			groupBox3.Controls.Add(this.chkHandrails);
			System.Windows.Forms.GroupBox groupBox4 = new System.Windows.Forms.GroupBox
			{
				Text = "Scaffolding Elevation & Ground Floor Logic",
				Location = new System.Drawing.Point(num, 410),
				Size = new System.Drawing.Size(num2, 130)
			};
			this.rdoAutoSlabBelow = new System.Windows.Forms.RadioButton
			{
				Text = "Auto-detect Slab Below (Extend prop from formwork to slab below top)",
				Location = new System.Drawing.Point(15, 25),
				AutoSize = true,
				Checked = true
			};
			this.rdoFixedHeight = new System.Windows.Forms.RadioButton
			{
				Text = "Fixed Prop Height (mm):",
				Location = new System.Drawing.Point(15, 52),
				AutoSize = true
			};
			this.numFixedHeight = new System.Windows.Forms.NumericUpDown
			{
				Location = new System.Drawing.Point(250, 49),
				Width = 90,
				Minimum = 500m,
				Maximum = 10000m,
				Value = 2800m,
				Increment = 100m,
				Enabled = false
			};
			this.chkSkipGroundProps = new System.Windows.Forms.CheckBox
			{
				Text = "Skip props under Lowest Ground Floor Slab (Resting on Grade)",
				Location = new System.Drawing.Point(15, 85),
				AutoSize = true,
				Checked = true
			};
			this.rdoFixedHeight.CheckedChanged += delegate
			{
				this.numFixedHeight.Enabled = this.rdoFixedHeight.Checked;
			};
			groupBox4.Controls.Add(this.rdoAutoSlabBelow);
			groupBox4.Controls.Add(this.rdoFixedHeight);
			groupBox4.Controls.Add(this.numFixedHeight);
			groupBox4.Controls.Add(this.chkSkipGroundProps);
			System.Windows.Forms.GroupBox groupBox5 = new System.Windows.Forms.GroupBox
			{
				Text = "Formwork Component Filter & Category Separation",
				Location = new System.Drawing.Point(num, 545),
				Size = new System.Drawing.Size(num2, 105)
			};
			this.chkModelPlywood = new System.Windows.Forms.CheckBox
			{
				Text = "Plywood Sheathing (Touching Concrete Face)",
				Location = new System.Drawing.Point(15, 22),
				AutoSize = true,
				Checked = true,
				Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
			};
			this.chkModelPrimaryJoists = new System.Windows.Forms.CheckBox
			{
				Text = "Primary H20 Joists",
				Location = new System.Drawing.Point(15, 48),
				AutoSize = true,
				Checked = true
			};
			this.chkModelSecondaryJoists = new System.Windows.Forms.CheckBox
			{
				Text = "Secondary Cross Joists",
				Location = new System.Drawing.Point(230, 48),
				AutoSize = true,
				Checked = true
			};
			this.chkModelWalers = new System.Windows.Forms.CheckBox
			{
				Text = "Steel Walers & Clamps",
				Location = new System.Drawing.Point(15, 74),
				AutoSize = true,
				Checked = true
			};
			this.chkModelProps = new System.Windows.Forms.CheckBox
			{
				Text = "Shoring & Push-Pull Props",
				Location = new System.Drawing.Point(230, 74),
				AutoSize = true,
				Checked = true
			};
			this.chkSeparateCategories = new System.Windows.Forms.CheckBox
			{
				Text = "Separate Revit Categories (Floors/Walls/Framing/Columns) for easy filter/selection",
				Location = new System.Drawing.Point(15, 96),
				AutoSize = true,
				Checked = true,
				ForeColor = System.Drawing.Color.FromArgb(0, 102, 204)
			};
			groupBox5.Controls.Add(this.chkModelPlywood);
			groupBox5.Controls.Add(this.chkModelPrimaryJoists);
			groupBox5.Controls.Add(this.chkModelSecondaryJoists);
			groupBox5.Controls.Add(this.chkModelWalers);
			groupBox5.Controls.Add(this.chkModelProps);
			groupBox5.Controls.Add(this.chkSeparateCategories);

			System.Windows.Forms.GroupBox groupBox6 = new System.Windows.Forms.GroupBox
			{
				Text = "CSV Quantity Report",
				Location = new System.Drawing.Point(num, 655),
				Size = new System.Drawing.Size(num2, 55)
			};
			this.chkExportCsv = new System.Windows.Forms.CheckBox
			{
				Text = "Automatically Prompt & Export CSV Quantity Relation Report",
				Location = new System.Drawing.Point(15, 22),
				AutoSize = true,
				Checked = true
			};
			groupBox6.Controls.Add(this.chkExportCsv);
			this.btnOk = new System.Windows.Forms.Button
			{
				Text = "Generate DOKA Formwork",
				DialogResult = System.Windows.Forms.DialogResult.OK,
				Location = new System.Drawing.Point(210, 725),
				Size = new System.Drawing.Size(170, 42),
				BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
				ForeColor = System.Drawing.Color.White,
				FlatStyle = System.Windows.Forms.FlatStyle.Flat
			};
			this.btnOk.FlatAppearance.BorderSize = 0;
			this.btnOk.Click += new System.EventHandler(BtnOk_Click);
			this.btnCancel = new System.Windows.Forms.Button
			{
				Text = "Cancel",
				DialogResult = System.Windows.Forms.DialogResult.Cancel,
				Location = new System.Drawing.Point(390, 725),
				Size = new System.Drawing.Size(120, 42),
				FlatStyle = System.Windows.Forms.FlatStyle.Flat
			};
			base.Controls.Add(groupBox);
			base.Controls.Add(groupBox2);
			base.Controls.Add(groupBox3);
			base.Controls.Add(groupBox4);
			base.Controls.Add(groupBox5);
			base.Controls.Add(groupBox6);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.btnCancel);
			base.AcceptButton = this.btnOk;
			base.CancelButton = this.btnCancel;
		}

		private void LoadSettingsToUI()
		{
			if (cmbPreset.Items.Contains(_settings.ColorPreset))
			{
				cmbPreset.SelectedItem = _settings.ColorPreset;
			}
			else
			{
				cmbPreset.SelectedIndex = 0;
			}
			chkSlabs.Checked = _settings.ProcessSlabs;
			chkBeams.Checked = _settings.ProcessBeams;
			chkColumns.Checked = _settings.ProcessColumns;
			chkWalls.Checked = _settings.ProcessWalls;
			numPanelThk.Value = (decimal)_settings.PanelThicknessMm;
			chkGenerateSupports.Checked = _settings.GenerateSupports;
			numPrimaryJoist.Value = (decimal)_settings.PrimaryJoistSpacingMm;
			numSecondaryJoist.Value = (decimal)_settings.SecondaryJoistSpacingMm;
			numPropGridX.Value = (decimal)_settings.ScaffoldPropGridXmm;
			numPropGridY.Value = (decimal)_settings.ScaffoldPropGridYmm;
			numWalerSpacing.Value = (decimal)_settings.WalerVerticalSpacingMm;
			chkTieRods.Checked = _settings.GenerateTieRods;
			chkHandrails.Checked = _settings.GeneratePerimeterHandrails;
			rdoAutoSlabBelow.Checked = _settings.AutoDetectSlabBelow;
			rdoFixedHeight.Checked = !_settings.AutoDetectSlabBelow;
			numFixedHeight.Value = (decimal)_settings.DefaultPropHeightMm;
			chkSkipGroundProps.Checked = _settings.SkipGroundSlabProps;
			chkExportCsv.Checked = _settings.AutoExportCsv;
			chkModelPlywood.Checked = _settings.ModelPlywood;
			chkModelPrimaryJoists.Checked = _settings.ModelPrimaryJoists;
			chkModelSecondaryJoists.Checked = _settings.ModelSecondaryJoists;
			chkModelWalers.Checked = _settings.ModelWalers;
			chkModelProps.Checked = _settings.ModelProps;
			chkSeparateCategories.Checked = _settings.SeparateCategories;
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			_settings.ColorPreset = cmbPreset.SelectedItem?.ToString() ?? "DOKA Official (Yellow / Blue / Steel)";
			_settings.ProcessSlabs = chkSlabs.Checked;
			_settings.ProcessBeams = chkBeams.Checked;
			_settings.ProcessColumns = chkColumns.Checked;
			_settings.ProcessWalls = chkWalls.Checked;
			_settings.PanelThicknessMm = (double)numPanelThk.Value;
			_settings.GenerateSupports = chkGenerateSupports.Checked;
			_settings.PrimaryJoistSpacingMm = (double)numPrimaryJoist.Value;
			_settings.SecondaryJoistSpacingMm = (double)numSecondaryJoist.Value;
			_settings.ScaffoldPropGridXmm = (double)numPropGridX.Value;
			_settings.ScaffoldPropGridYmm = (double)numPropGridY.Value;
			_settings.WalerVerticalSpacingMm = (double)numWalerSpacing.Value;
			_settings.GenerateTieRods = chkTieRods.Checked;
			_settings.GeneratePerimeterHandrails = chkHandrails.Checked;
			_settings.AutoDetectSlabBelow = rdoAutoSlabBelow.Checked;
			_settings.DefaultPropHeightMm = (double)numFixedHeight.Value;
			_settings.SkipGroundSlabProps = chkSkipGroundProps.Checked;
			_settings.AutoExportCsv = chkExportCsv.Checked;
			_settings.ModelPlywood = chkModelPlywood.Checked;
			_settings.ModelPrimaryJoists = chkModelPrimaryJoists.Checked;
			_settings.ModelSecondaryJoists = chkModelSecondaryJoists.Checked;
			_settings.ModelWalers = chkModelWalers.Checked;
			_settings.ModelProps = chkModelProps.Checked;
			_settings.SeparateCategories = chkSeparateCategories.Checked;
		}
	}
}
