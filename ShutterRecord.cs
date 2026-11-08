namespace RevitShutteringAddin
{
	public class ShutterRecord
	{
		public string HostElementId { get; set; }

		public string HostCategory { get; set; }

		public string ShutterElementId { get; set; }

		public string ShutterMarkValue { get; set; }

		public double ShutterArea { get; set; }

		public double ElementWidth { get; set; }

		public double ElementHeight { get; set; }

		public double ElementLength { get; set; }

		public double ElementThickness { get; set; }
	}
}
