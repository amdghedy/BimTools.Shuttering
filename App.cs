using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;

namespace RevitShutteringAddin
{
	public class App : IExternalApplication
	{
		public Result OnStartup(UIControlledApplication application)
		{
			string tabName = "BIM TOOLS";
			string text = "Model Tool";
			string text2 = "shuttering";
			try
			{
				application.CreateRibbonTab(tabName);
			}
			catch (Autodesk.Revit.Exceptions.ArgumentException)
			{
			}
			catch
			{
			}
			RibbonPanel ribbonPanel = null;
			try
			{
				List<RibbonPanel> ribbonPanels = application.GetRibbonPanels(tabName);
				foreach (RibbonPanel item in ribbonPanels)
				{
					if (item.Name.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						ribbonPanel = item;
						break;
					}
				}
			}
			catch
			{
			}
			if (ribbonPanel == null)
			{
				try
				{
					ribbonPanel = application.CreateRibbonPanel(tabName, text);
				}
				catch
				{
					try
					{
						ribbonPanel = application.CreateRibbonPanel(text);
					}
					catch
					{
					}
				}
			}
			if (ribbonPanel == null)
			{
				return Result.Failed;
			}
			try
			{
				string location = Assembly.GetExecutingAssembly().Location;
				PushButtonData itemData = new PushButtonData(text2, text2, location, "RevitShutteringAddin.Command")
				{
					ToolTip = "Generate shuttering elements for slabs, beams, columns, and walls in the active view."
				};
				ribbonPanel.AddSeparator();
				ribbonPanel.AddItem(itemData);
			}
			catch
			{
				return Result.Failed;
			}
			return Result.Succeeded;
		}

		public Result OnShutdown(UIControlledApplication application)
		{
			return Result.Succeeded;
		}
	}
}
