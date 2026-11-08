namespace RevitShutteringAddin
{
	public class ShutteringSettings
	{
		public bool ProcessSlabs { get; set; } = true;

		public bool ProcessBeams { get; set; } = true;

		public bool ProcessColumns { get; set; } = true;

		public bool ProcessWalls { get; set; } = true;

		public double PanelThicknessMm { get; set; } = 20.0;

		public bool GenerateSupports { get; set; } = true;

		public double PrimaryJoistSpacingMm { get; set; } = 500.0;

		public double SecondaryJoistSpacingMm { get; set; } = 1000.0;

		public double ScaffoldPropGridXmm { get; set; } = 1200.0;

		public double ScaffoldPropGridYmm { get; set; } = 1200.0;

		public double WalerVerticalSpacingMm { get; set; } = 600.0;

		public bool AutoDetectSlabBelow { get; set; } = true;

		public double DefaultPropHeightMm { get; set; } = 2800.0;

		public bool SkipGroundSlabProps { get; set; } = true;

		public bool GenerateTieRods { get; set; } = true;

		public bool GeneratePerimeterHandrails { get; set; } = true;

		public string ColorPreset { get; set; } = "DOKA Official (Yellow / Blue / Steel)";

		public bool AutoExportCsv { get; set; } = true;
	}
}
