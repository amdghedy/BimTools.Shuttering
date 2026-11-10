using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitShutteringAddin
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class Command : IExternalCommand
	{
		private const double ShutteringThicknessMm = 20.0;

		private const double FeetToMm = 304.8;

		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIDocument activeUIDocument = commandData.Application.ActiveUIDocument;
			Document document = activeUIDocument.Document;
			Autodesk.Revit.DB.View activeView = document.ActiveView;
			if (activeView == null)
			{
				message = "No active view found.";
				return Result.Failed;
			}
			ShutteringSettings shutteringSettings = new ShutteringSettings();
			using (SettingsForm settingsForm = new SettingsForm(shutteringSettings))
			{
				IWin32Window owner = new WindowHandle(commandData.Application.MainWindowHandle);
				if (settingsForm.ShowDialog(owner) != DialogResult.OK)
				{
					return Result.Cancelled;
				}
			}
			List<ShutterRecord> list = new List<ShutterRecord>();
			List<Element> list2 = new List<Element>();
			List<Element> list3 = new List<Element>();
			List<Element> list4 = new List<Element>();
			List<Element> list5 = new List<Element>();
			FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document, activeView.Id);
			foreach (Element item in filteredElementCollector)
			{
				if (item.Category != null)
				{
					long catId = RevitUtilities.GetIdValue(item.Category.Id);
					if (shutteringSettings.ProcessSlabs && catId == -2000032)
					{
						list2.Add(item);
					}
					else if (shutteringSettings.ProcessBeams && catId == -2001320)
					{
						list3.Add(item);
					}
					else if (shutteringSettings.ProcessColumns && catId == -2001330)
					{
						list4.Add(item);
					}
					else if (shutteringSettings.ProcessWalls && catId == -2000011)
					{
						list5.Add(item);
					}
				}
			}
			if (list2.Count == 0 && list3.Count == 0 && list4.Count == 0 && list5.Count == 0)
			{
				TaskDialog.Show("Shuttering Tool", "No slabs, beams, columns, or walls found in the current view.");
				return Result.Succeeded;
			}
			Dictionary<ElementId, List<Solid>> solidsMap = new Dictionary<ElementId, List<Solid>>();
			foreach (Element item2 in list2)
			{
				solidsMap[item2.Id] = GetElementSolids(item2);
			}
			foreach (Element item3 in list3)
			{
				solidsMap[item3.Id] = GetElementSolids(item3);
			}
			foreach (Element item4 in list4)
			{
				solidsMap[item4.Id] = GetElementSolids(item4);
			}
			foreach (Element item5 in list5)
			{
				solidsMap[item5.Id] = GetElementSolids(item5);
			}
			List<Solid> list6 = list3.SelectMany((Element e) => solidsMap.ContainsKey(e.Id) ? solidsMap[e.Id] : new List<Solid>()).ToList();
			List<Solid> list7 = list4.SelectMany((Element e) => solidsMap.ContainsKey(e.Id) ? solidsMap[e.Id] : new List<Solid>()).ToList();
			List<Solid> list8 = list2.SelectMany((Element e) => solidsMap.ContainsKey(e.Id) ? solidsMap[e.Id] : new List<Solid>()).ToList();
			List<Solid> list9 = list5.SelectMany((Element e) => solidsMap.ContainsKey(e.Id) ? solidsMap[e.Id] : new List<Solid>()).ToList();
			double thicknessFeet = shutteringSettings.PanelThicknessMm / 304.8;
			int num = 0;
			using (Transaction transaction = new Transaction(document, "Create Shuttering"))
			{
				transaction.Start();
				List<string> list10 = new List<string>
				{
					"Slab Under Shuttering", "Slab Side Shuttering", "Beam Bottom Shuttering", "Beam Side Shuttering", "Column Shuttering", "Wall Shuttering", "Slab Primary Joist", "Slab Secondary Joist", "Slab Scaffold Prop", "Wall Waler",
					"Wall Push-Pull Prop", "Column Waler", "Column Support Prop", "Beam Bottom Support", "Beam Support Prop", "DOKA Dywidag Tie Rod", "DOKA Wing Nut Plate", "DOKA Perimeter Safety Post"
				};
				FilteredElementCollector source = new FilteredElementCollector(document, activeView.Id).OfClass(typeof(DirectShape));
				List<ElementId> list11 = new List<ElementId>();
				foreach (DirectShape item6 in source.Cast<DirectShape>())
				{
					if (list10.Contains(item6.Name))
					{
						list11.Add(item6.Id);
					}
				}
				if (list11.Count > 0)
				{
					try
					{
						document.Delete(list11);
					}
					catch
					{
					}
				}
				double globalMinSlabZ = ((list8.Count > 0) ? list8.Min((Solid s) => GetSolidBoundingBox(s)?.Min.Z ?? double.MaxValue) : double.MinValue);
				Color color = new Color(245, 190, 20);
				Color color2 = new Color(250, 210, 30);
				Color color3 = new Color(0, 75, 150);
				Color color4 = new Color(190, 195, 200);
				Color color5 = new Color(60, 65, 70);
				if (shutteringSettings.ColorPreset != null && shutteringSettings.ColorPreset.StartsWith("PERI", StringComparison.OrdinalIgnoreCase))
				{
					color = new Color(220, 50, 40);
					color2 = new Color(245, 200, 30);
					color3 = new Color(210, 40, 40);
					color4 = new Color(180, 185, 190);
				}
				else if (shutteringSettings.ColorPreset != null && shutteringSettings.ColorPreset.StartsWith("Universal", StringComparison.OrdinalIgnoreCase))
				{
					color = new Color(180, 130, 80);
					color2 = new Color(240, 195, 45);
					color3 = new Color(70, 80, 95);
					color4 = new Color(220, 75, 40);
				}
				Material orCreateMaterial = GetOrCreateMaterial(document, "Formwork - Shuttering Panel", color);
				Material orCreateMaterial2 = GetOrCreateMaterial(document, "Formwork - Timber H20 Joist", color2);
				Material orCreateMaterial3 = GetOrCreateMaterial(document, "Formwork - Steel Waler Frame", color3);
				Material orCreateMaterial4 = GetOrCreateMaterial(document, "Formwork - Steel Scaffold Prop", color4);
				Material orCreateMaterial5 = GetOrCreateMaterial(document, "Formwork - Dywidag Tie Rod", color5);
				List<Solid> list12 = new List<Solid>();
				List<Solid> list13 = new List<Solid>();
				foreach (Floor slab in list2.OfType<Floor>())
				{
					if (!solidsMap.ContainsKey(slab.Id))
					{
						continue;
					}
					double a = 0.0;
					CompoundStructure compoundStructure = slab.FloorType.GetCompoundStructure();
					if (compoundStructure != null)
					{
						a = compoundStructure.GetWidth() * 304.8;
					}
					string arg = $"{Math.Round(a)}mm";
					double a2 = 0.0;
					double a3 = 0.0;
					BoundingBoxXYZ boundingBoxXYZ = ((Element)slab).get_BoundingBox((Autodesk.Revit.DB.View)null);
					if (boundingBoxXYZ != null)
					{
						double val = (boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X) * 304.8;
						double val2 = (boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y) * 304.8;
						a2 = Math.Max(val, val2);
						a3 = Math.Min(val, val2);
					}
					List<Solid> list14 = solidsMap[slab.Id];
					foreach (Solid item7 in list14)
					{
						foreach (Face face5 in item7.Faces)
						{
							if (!(face5 is PlanarFace { FaceNormal: var faceNormal } planarFace))
							{
								continue;
							}
							if (faceNormal.Z < -0.5)
							{
								if (!shutteringSettings.ModelPlywood)
								{
									continue;
								}
								Solid solid = CreateShutteringSolid(planarFace, thicknessFeet);
								if (solid == null)
								{
									continue;
								}
								List<Solid> cutters = list6.Concat(list7).Concat(list12).ToList();
								solid = SubtractSolids(solid, cutters);
								if (solid != null && solid.Volume > 1E-05)
								{
									double value = solid.Volume * 1.4158428;
									string text = $"Slab Under Shuttering - {arg} | Area: {Math.Round(value, 2)} m²";
									BuiltInCategory cat = shutteringSettings.SeparateCategories ? BuiltInCategory.OST_Floors : BuiltInCategory.OST_GenericModel;
									ElementId elementId = CreateDirectShape(document, solid, "Slab Under Shuttering", text, orCreateMaterial, cat);
									if (elementId != ElementId.InvalidElementId)
									{
										list.Add(new ShutterRecord
										{
											HostElementId = slab.Id.ToString(),
											HostCategory = slab.Category.Name,
											ShutterElementId = elementId.ToString(),
											ComponentType = "Plywood (Touching Concrete)",
											ShutterMarkValue = text,
											ShutterArea = Math.Round(value, 2),
											ElementWidth = Math.Round(a3),
											ElementLength = Math.Round(a2),
											ElementThickness = Math.Round(a),
											ElementHeight = 0.0
										});
										num++;
									}
								}
							}
							else
							{
								if (!(Math.Abs(faceNormal.Z) < 0.1))
								{
									continue;
								}
								if (!shutteringSettings.ModelPlywood)
								{
									continue;
								}
								Solid solid2 = CreateShutteringSolid(planarFace, thicknessFeet);
								if (solid2 == null)
								{
									continue;
								}
								List<Solid> second = list2.Where((Element s) => s.Id != slab.Id).SelectMany((Element s) => solidsMap.ContainsKey(s.Id) ? solidsMap[s.Id] : new List<Solid>()).ToList();
								List<Solid> cutters2 = list7.Concat(list6).Concat(list9).Concat(second)
									.ToList();
								solid2 = SubtractSolids(solid2, cutters2);
								if (solid2 != null && solid2.Volume > 1E-05)
								{
									double value2 = solid2.Volume * 1.4158428;
									string text2 = $"Slab Side Shuttering - {arg} | Area: {Math.Round(value2, 2)} m²";
									BuiltInCategory cat2 = shutteringSettings.SeparateCategories ? BuiltInCategory.OST_Floors : BuiltInCategory.OST_GenericModel;
									ElementId elementId2 = CreateDirectShape(document, solid2, "Slab Side Shuttering", text2, orCreateMaterial, cat2);
									if (elementId2 != ElementId.InvalidElementId)
									{
										list.Add(new ShutterRecord
										{
											HostElementId = slab.Id.ToString(),
											HostCategory = slab.Category.Name,
											ShutterElementId = elementId2.ToString(),
											ComponentType = "Plywood (Touching Concrete)",
											ShutterMarkValue = text2,
											ShutterArea = Math.Round(value2, 2),
											ElementWidth = Math.Round(a3),
											ElementLength = Math.Round(a2),
											ElementThickness = Math.Round(a),
											ElementHeight = 0.0
										});
										list13.Add(solid2);
										num++;
									}
								}
							}
						}
					}
					if (shutteringSettings.GenerateSupports)
					{
						num += CreateSlabSupports(document, slab, list14, list8, list6, list7, list9, globalMinSlabZ, orCreateMaterial2, orCreateMaterial4, list, shutteringSettings);
					}
					list12.AddRange(list14);
				}
				List<Solid> list15 = new List<Solid>();
				foreach (FamilyInstance item8 in list3.OfType<FamilyInstance>())
				{
					if (!solidsMap.ContainsKey(item8.Id))
					{
						continue;
					}
					string beamSizeString = GetBeamSizeString(item8);
					GetBeamDimensions(item8, out var width, out var height);
					double a4 = 0.0;
					BoundingBoxXYZ boundingBoxXYZ2 = ((Element)item8).get_BoundingBox((Autodesk.Revit.DB.View)null);
					if (boundingBoxXYZ2 != null)
					{
						double val3 = (boundingBoxXYZ2.Max.X - boundingBoxXYZ2.Min.X) * 304.8;
						double val4 = (boundingBoxXYZ2.Max.Y - boundingBoxXYZ2.Min.Y) * 304.8;
						a4 = Math.Max(val3, val4);
					}
					List<Solid> list16 = solidsMap[item8.Id];
					foreach (Solid item9 in list16)
					{
						foreach (Face face6 in item9.Faces)
						{
							if (!(face6 is PlanarFace { FaceNormal: var faceNormal2 } planarFace2))
							{
								continue;
							}
							if (faceNormal2.Z < -0.5)
							{
								if (!shutteringSettings.ModelPlywood)
								{
									continue;
								}
								Solid solid3 = CreateShutteringSolid(planarFace2, thicknessFeet);
								if (solid3 == null)
								{
									continue;
								}
								List<Solid> cutters3 = list7.Concat(list15).ToList();
								solid3 = SubtractSolids(solid3, cutters3);
								if (solid3 != null && solid3.Volume > 1E-05)
								{
									double value3 = solid3.Volume * 1.4158428;
									string text3 = $"Beam Bottom Shuttering - {beamSizeString} | Area: {Math.Round(value3, 2)} m²";
									BuiltInCategory cat3 = shutteringSettings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
									ElementId elementId3 = CreateDirectShape(document, solid3, "Beam Bottom Shuttering", text3, orCreateMaterial, cat3);
									if (elementId3 != ElementId.InvalidElementId)
									{
										list.Add(new ShutterRecord
										{
											HostElementId = item8.Id.ToString(),
											HostCategory = item8.Category.Name,
											ShutterElementId = elementId3.ToString(),
											ComponentType = "Plywood (Touching Concrete)",
											ShutterMarkValue = text3,
											ShutterArea = Math.Round(value3, 2),
											ElementWidth = Math.Round(width),
											ElementLength = Math.Round(a4),
											ElementThickness = 0.0,
											ElementHeight = Math.Round(height)
										});
										num++;
									}
								}
							}
							else
							{
								if (!(Math.Abs(faceNormal2.Z) <= 0.5))
								{
									continue;
								}
								if (!shutteringSettings.ModelPlywood)
								{
									continue;
								}
								Solid solid4 = CreateShutteringSolid(planarFace2, thicknessFeet);
								if (solid4 == null)
								{
									continue;
								}
								List<Solid> cutters4 = list7.Concat(list8).Concat(list13).Concat(list15)
									.ToList();
								solid4 = SubtractSolids(solid4, cutters4);
								if (solid4 != null && solid4.Volume > 1E-05)
								{
									double value4 = solid4.Volume * 1.4158428;
									string text4 = $"Beam Side Shuttering - {beamSizeString} | Area: {Math.Round(value4, 2)} m²";
									BuiltInCategory cat4 = shutteringSettings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
									ElementId elementId4 = CreateDirectShape(document, solid4, "Beam Side Shuttering", text4, orCreateMaterial, cat4);
									if (elementId4 != ElementId.InvalidElementId)
									{
										list.Add(new ShutterRecord
										{
											HostElementId = item8.Id.ToString(),
											HostCategory = item8.Category.Name,
											ShutterElementId = elementId4.ToString(),
											ComponentType = "Plywood (Touching Concrete)",
											ShutterMarkValue = text4,
											ShutterArea = Math.Round(value4, 2),
											ElementWidth = Math.Round(width),
											ElementLength = Math.Round(a4),
											ElementThickness = 0.0,
											ElementHeight = Math.Round(height)
										});
										num++;
									}
								}
							}
						}
					}
					if (shutteringSettings.GenerateSupports)
					{
						num += CreateBeamSupports(document, item8, list16, list8, list7, list9, globalMinSlabZ, orCreateMaterial3, orCreateMaterial4, list, shutteringSettings);
					}
					list15.AddRange(list16);
				}
				List<Solid> list17 = new List<Solid>();
				foreach (FamilyInstance item10 in list4.OfType<FamilyInstance>())
				{
					if (!solidsMap.ContainsKey(item10.Id))
					{
						continue;
					}
					string columnSizeString = GetColumnSizeString(item10);
					GetColumnDimensions(item10, out var width2, out var depth);
					double a5 = 0.0;
					BoundingBoxXYZ boundingBoxXYZ3 = ((Element)item10).get_BoundingBox((Autodesk.Revit.DB.View)null);
					if (boundingBoxXYZ3 != null)
					{
						a5 = (boundingBoxXYZ3.Max.Z - boundingBoxXYZ3.Min.Z) * 304.8;
					}
					List<Solid> list18 = solidsMap[item10.Id];
					foreach (Solid item11 in list18)
					{
						foreach (Face face7 in item11.Faces)
						{
							if (!(face7 is PlanarFace { FaceNormal: var faceNormal3 } planarFace3) || !(Math.Abs(faceNormal3.Z) < 0.1))
							{
								continue;
							}
							if (!shutteringSettings.ModelPlywood)
							{
								continue;
							}
							Solid solid5 = CreateShutteringSolid(planarFace3, thicknessFeet);
							if (solid5 == null)
							{
								continue;
							}
							List<Solid> cutters5 = list8.Concat(list6).Concat(list9).Concat(list17)
								.ToList();
							solid5 = SubtractSolids(solid5, cutters5);
							if (solid5 != null && solid5.Volume > 1E-05)
							{
								double value5 = solid5.Volume * 1.4158428;
								string text5 = $"Column Shuttering - {columnSizeString} | Area: {Math.Round(value5, 2)} m²";
								BuiltInCategory cat5 = shutteringSettings.SeparateCategories ? BuiltInCategory.OST_Columns : BuiltInCategory.OST_GenericModel;
								ElementId elementId5 = CreateDirectShape(document, solid5, "Column Shuttering", text5, orCreateMaterial, cat5);
								if (elementId5 != ElementId.InvalidElementId)
								{
									list.Add(new ShutterRecord
									{
										HostElementId = item10.Id.ToString(),
										HostCategory = item10.Category.Name,
										ShutterElementId = elementId5.ToString(),
										ComponentType = "Plywood (Touching Concrete)",
										ShutterMarkValue = text5,
										ShutterArea = Math.Round(value5, 2),
										ElementWidth = Math.Round(width2),
										ElementLength = Math.Round(depth),
										ElementThickness = 0.0,
										ElementHeight = Math.Round(a5)
									});
									num++;
								}
							}
						}
					}
					if (shutteringSettings.GenerateSupports)
					{
						num += CreateColumnSupports(document, item10, list18, orCreateMaterial3, orCreateMaterial4, orCreateMaterial5, list, shutteringSettings);
					}
					list17.AddRange(list18);
				}
				List<Solid> list19 = new List<Solid>();
				foreach (Wall wall in list5.OfType<Wall>())
				{
					if (!solidsMap.ContainsKey(wall.Id))
					{
						continue;
					}
					double a6 = wall.WallType.Width * 304.8;
					string arg2 = $"{Math.Round(a6)}mm";
					double a7 = 0.0;
					double a8 = 0.0;
					BoundingBoxXYZ boundingBoxXYZ4 = ((Element)wall).get_BoundingBox((Autodesk.Revit.DB.View)null);
					if (boundingBoxXYZ4 != null)
					{
						double val5 = (boundingBoxXYZ4.Max.X - boundingBoxXYZ4.Min.X) * 304.8;
						double val6 = (boundingBoxXYZ4.Max.Y - boundingBoxXYZ4.Min.Y) * 304.8;
						a7 = (boundingBoxXYZ4.Max.Z - boundingBoxXYZ4.Min.Z) * 304.8;
						a8 = Math.Max(val5, val6);
					}
					List<Solid> list20 = solidsMap[wall.Id];
					foreach (Solid item12 in list20)
					{
						foreach (Face face8 in item12.Faces)
						{
							if (!(face8 is PlanarFace { FaceNormal: var faceNormal4 } planarFace4) || !(Math.Abs(faceNormal4.Z) < 0.1))
							{
								continue;
							}
							if (!shutteringSettings.ModelPlywood)
							{
								continue;
							}
							Solid solid6 = CreateShutteringSolid(planarFace4, thicknessFeet);
							if (solid6 == null)
							{
								continue;
							}
							List<Solid> second2 = list5.Where((Element w) => w.Id != wall.Id).SelectMany((Element w) => solidsMap.ContainsKey(w.Id) ? solidsMap[w.Id] : new List<Solid>()).ToList();
							List<Solid> cutters6 = list8.Concat(list6).Concat(list7).Concat(second2)
								.Concat(list19)
								.ToList();
							solid6 = SubtractSolids(solid6, cutters6);
							if (solid6 != null && solid6.Volume > 1E-05)
							{
								double value6 = solid6.Volume * 1.4158428;
								string text6 = $"Wall Shuttering - {arg2} | Area: {Math.Round(value6, 2)} m²";
								BuiltInCategory cat6 = shutteringSettings.SeparateCategories ? BuiltInCategory.OST_Walls : BuiltInCategory.OST_GenericModel;
								ElementId elementId6 = CreateDirectShape(document, solid6, "Wall Shuttering", text6, orCreateMaterial, cat6);
								if (elementId6 != ElementId.InvalidElementId)
								{
									list.Add(new ShutterRecord
									{
										HostElementId = wall.Id.ToString(),
										HostCategory = wall.Category.Name,
										ShutterElementId = elementId6.ToString(),
										ComponentType = "Plywood (Touching Concrete)",
										ShutterMarkValue = text6,
										ShutterArea = Math.Round(value6, 2),
										ElementWidth = 0.0,
										ElementLength = Math.Round(a8),
										ElementThickness = Math.Round(a6),
										ElementHeight = Math.Round(a7)
									});
									num++;
								}
							}
						}
					}
					if (shutteringSettings.GenerateSupports)
					{
						num += CreateWallSupports(document, wall, list20, orCreateMaterial3, orCreateMaterial4, orCreateMaterial5, list, shutteringSettings);
					}
					list19.AddRange(list20);
				}
				transaction.Commit();
			}
			if (shutteringSettings.AutoExportCsv && list.Count > 0)
			{
				ExportCsv(commandData.Application, list);
			}
			TaskDialog.Show("Shuttering Tool", $"Successfully created {num} shuttering elements (Generic Models) in the current view.");
			return Result.Succeeded;
		}

		private static List<Solid> GetElementSolids(Element elem)
		{
			List<Solid> list = new List<Solid>();
			Options options = new Options
			{
				ComputeReferences = true,
				DetailLevel = ViewDetailLevel.Fine
			};
			try
			{
				GeometryElement geometryElement = elem.get_Geometry(options);
				if (geometryElement != null)
				{
					AddSolids(geometryElement, list);
				}
			}
			catch
			{
			}
			return list;
		}

		private static void AddSolids(GeometryObject geomObj, List<Solid> solids)
		{
			if (geomObj is Solid solid)
			{
				if (solid.Volume > 1E-05)
				{
					solids.Add(solid);
				}
				return;
			}
			if (geomObj is GeometryInstance geometryInstance)
			{
				GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();
				{
					foreach (GeometryObject item in instanceGeometry)
					{
						AddSolids(item, solids);
					}
					return;
				}
			}
			if (!(geomObj is GeometryElement geometryElement))
			{
				return;
			}
			foreach (GeometryObject item2 in geometryElement)
			{
				AddSolids(item2, solids);
			}
		}

		private static Solid CreateShutteringSolid(PlanarFace face, double thicknessFeet)
		{
			try
			{
				IList<CurveLoop> edgesAsCurveLoops = face.GetEdgesAsCurveLoops();
				IList<CurveLoop> list = SanitizeCurveLoops(edgesAsCurveLoops);
				if (list.Count == 0)
				{
					return null;
				}
				XYZ faceNormal = face.FaceNormal;
				return GeometryCreationUtilities.CreateExtrusionGeometry(list, faceNormal, thicknessFeet);
			}
			catch
			{
				return null;
			}
		}

		private static IList<CurveLoop> SanitizeCurveLoops(IList<CurveLoop> loops)
		{
			List<CurveLoop> list = new List<CurveLoop>();
			foreach (CurveLoop loop in loops)
			{
				CurveLoop curveLoop = new CurveLoop();
				bool flag = false;
				foreach (Curve item in loop)
				{
					if (item.Length < 0.006)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(loop);
					continue;
				}
				List<XYZ> list2 = new List<XYZ>();
				foreach (Curve item2 in loop)
				{
					if (item2.Length >= 0.006)
					{
						list2.Add(item2.GetEndPoint(0));
					}
				}
				if (list2.Count < 3)
				{
					continue;
				}
				for (int i = 0; i < list2.Count; i++)
				{
					XYZ xYZ = list2[i];
					XYZ xYZ2 = list2[(i + 1) % list2.Count];
					if (xYZ.DistanceTo(xYZ2) >= 0.006)
					{
						curveLoop.Append(Line.CreateBound(xYZ, xYZ2));
					}
				}
				if (!curveLoop.IsOpen())
				{
					list.Add(curveLoop);
				}
			}
			return list;
		}

		private static Solid SubtractSolids(Solid baseSolid, List<Solid> cutters)
		{
			Solid solid = baseSolid;
			foreach (Solid cutter in cutters)
			{
				if (cutter == null || cutter.Volume < 1E-05)
				{
					continue;
				}
				try
				{
					Solid solid2 = BooleanOperationsUtils.ExecuteBooleanOperation(solid, cutter, BooleanOperationsType.Difference);
					if (solid2 != null && solid2.Volume > 1E-05)
					{
						solid = solid2;
					}
				}
				catch
				{
				}
			}
			return solid;
		}

		private static ElementId CreateDirectShape(Document doc, Solid solid, string name, string comment, Material material, BuiltInCategory builtInCat = BuiltInCategory.OST_GenericModel)
		{
			try
			{
				DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(builtInCat));
				directShape.Name = name;
				directShape.SetShape(new List<GeometryObject> { solid });
				double value = solid.Volume * 1.4158428;
				string value2 = $"{comment} | Area: {Math.Round(value, 2)} m²";
				Parameter parameter = ((Element)directShape).get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
				if (parameter != null && !parameter.IsReadOnly)
				{
					parameter.Set(value2);
				}
				Parameter markParam = ((Element)directShape).get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
				if (markParam != null && !markParam.IsReadOnly)
				{
					markParam.Set(name);
				}
				if (material != null)
				{
					Parameter parameter2 = ((Element)directShape).get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
					if (parameter2 != null && !parameter2.IsReadOnly)
					{
						parameter2.Set(material.Id);
					}
					else
					{
						foreach (Parameter parameter3 in directShape.Parameters)
						{
							if (parameter3.Definition.Name.Equals("Material", StringComparison.OrdinalIgnoreCase) && !parameter3.IsReadOnly)
							{
								parameter3.Set(material.Id);
								break;
							}
						}
					}
				}
				return directShape.Id;
			}
			catch
			{
				return ElementId.InvalidElementId;
			}
		}

		private static Material GetOrCreateShutteringMaterial(Document doc)
		{
			Material material = new FilteredElementCollector(doc).OfClass(typeof(Material)).Cast<Material>().FirstOrDefault((Material m) => m.Name.Equals("Wood - Shuttering", StringComparison.OrdinalIgnoreCase));
			if (material == null)
			{
				material = new FilteredElementCollector(doc).OfClass(typeof(Material)).Cast<Material>().FirstOrDefault((Material m) => m.Name.IndexOf("wood", StringComparison.OrdinalIgnoreCase) >= 0 || m.Name.IndexOf("timber", StringComparison.OrdinalIgnoreCase) >= 0);
			}
			if (material == null)
			{
				ElementId id = Material.Create(doc, "Wood - Shuttering");
				material = doc.GetElement(id) as Material;
				if (material != null)
				{
					material.Color = new Color(205, 170, 125);
				}
			}
			return material;
		}

		private static string GetBeamSizeString(FamilyInstance beam)
		{
			double num = 0.0;
			double num2 = 0.0;
			FamilySymbol symbol = beam.Symbol;
			Parameter parameter = symbol.LookupParameter("b") ?? symbol.LookupParameter("Width");
			Parameter parameter2 = symbol.LookupParameter("h") ?? symbol.LookupParameter("d") ?? symbol.LookupParameter("Height");
			if (parameter != null)
			{
				num = parameter.AsDouble() * 304.8;
			}
			if (parameter2 != null)
			{
				num2 = parameter2.AsDouble() * 304.8;
			}
			if (num == 0.0 || num2 == 0.0)
			{
				BoundingBoxXYZ boundingBoxXYZ = ((Element)beam).get_BoundingBox((Autodesk.Revit.DB.View)null);
				if (boundingBoxXYZ != null)
				{
					double val = (boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X) * 304.8;
					double val2 = (boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y) * 304.8;
					double num3 = (boundingBoxXYZ.Max.Z - boundingBoxXYZ.Min.Z) * 304.8;
					num = Math.Min(val, val2);
					num2 = num3;
				}
			}
			return $"{Math.Round(num)}x{Math.Round(num2)}";
		}

		private static void GetBeamDimensions(FamilyInstance beam, out double width, out double height)
		{
			width = 0.0;
			height = 0.0;
			FamilySymbol symbol = beam.Symbol;
			Parameter parameter = symbol.LookupParameter("b") ?? symbol.LookupParameter("Width");
			Parameter parameter2 = symbol.LookupParameter("h") ?? symbol.LookupParameter("d") ?? symbol.LookupParameter("Height");
			if (parameter != null)
			{
				width = parameter.AsDouble() * 304.8;
			}
			if (parameter2 != null)
			{
				height = parameter2.AsDouble() * 304.8;
			}
			if (width == 0.0 || height == 0.0)
			{
				BoundingBoxXYZ boundingBoxXYZ = ((Element)beam).get_BoundingBox((Autodesk.Revit.DB.View)null);
				if (boundingBoxXYZ != null)
				{
					double val = (boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X) * 304.8;
					double val2 = (boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y) * 304.8;
					double num = (boundingBoxXYZ.Max.Z - boundingBoxXYZ.Min.Z) * 304.8;
					width = Math.Min(val, val2);
					height = num;
				}
			}
		}

		private static string GetColumnSizeString(FamilyInstance col)
		{
			double num = 0.0;
			double num2 = 0.0;
			FamilySymbol symbol = col.Symbol;
			Parameter parameter = symbol.LookupParameter("b") ?? symbol.LookupParameter("Width");
			Parameter parameter2 = symbol.LookupParameter("h") ?? symbol.LookupParameter("Depth") ?? symbol.LookupParameter("d");
			if (parameter != null)
			{
				num = parameter.AsDouble() * 304.8;
			}
			if (parameter2 != null)
			{
				num2 = parameter2.AsDouble() * 304.8;
			}
			if (num == 0.0 || num2 == 0.0)
			{
				BoundingBoxXYZ boundingBoxXYZ = ((Element)col).get_BoundingBox((Autodesk.Revit.DB.View)null);
				if (boundingBoxXYZ != null)
				{
					num = (boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X) * 304.8;
					num2 = (boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y) * 304.8;
				}
			}
			return $"{Math.Round(num)}x{Math.Round(num2)}";
		}

		private static void GetColumnDimensions(FamilyInstance col, out double width, out double depth)
		{
			width = 0.0;
			depth = 0.0;
			FamilySymbol symbol = col.Symbol;
			Parameter parameter = symbol.LookupParameter("b") ?? symbol.LookupParameter("Width");
			Parameter parameter2 = symbol.LookupParameter("h") ?? symbol.LookupParameter("Depth") ?? symbol.LookupParameter("d");
			if (parameter != null)
			{
				width = parameter.AsDouble() * 304.8;
			}
			if (parameter2 != null)
			{
				depth = parameter2.AsDouble() * 304.8;
			}
			if (width == 0.0 || depth == 0.0)
			{
				BoundingBoxXYZ boundingBoxXYZ = ((Element)col).get_BoundingBox((Autodesk.Revit.DB.View)null);
				if (boundingBoxXYZ != null)
				{
					width = (boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X) * 304.8;
					depth = (boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y) * 304.8;
				}
			}
		}

		private static void ExportCsv(UIApplication uiApp, List<ShutterRecord> records)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			try
			{
				saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
				saveFileDialog.Title = "Export Shuttering Data";
				saveFileDialog.FileName = "ShutteringReport.csv";
				IWin32Window owner = new WindowHandle(uiApp.MainWindowHandle);
				if (saveFileDialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}
				try
				{
					using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, append: false, Encoding.UTF8))
					{
						streamWriter.WriteLine("Element ID,Element Category,Element Width (mm),Element Height (mm),Element Length (mm),Element Thickness (mm),Shutter ID,Shutter Mark Value,Area (m²)");
						foreach (ShutterRecord record in records)
						{
							string text = record.ShutterMarkValue;
							if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
							{
								text = "\"" + text.Replace("\"", "\"\"") + "\"";
							}
							string text2 = ((record.ElementWidth > 0.0) ? record.ElementWidth.ToString() : "");
							string text3 = ((record.ElementHeight > 0.0) ? record.ElementHeight.ToString() : "");
							string text4 = ((record.ElementLength > 0.0) ? record.ElementLength.ToString() : "");
							string text5 = ((record.ElementThickness > 0.0) ? record.ElementThickness.ToString() : "");
							streamWriter.WriteLine($"{record.HostElementId},{record.HostCategory},{text2},{text3},{text4},{text5},{record.ShutterElementId},{text},{record.ShutterArea}");
						}
					}
					TaskDialog.Show("CSV Export", $"Successfully exported {records.Count} records to {saveFileDialog.FileName}");
				}
				catch (Exception ex)
				{
					TaskDialog.Show("CSV Export Error", "Failed to export CSV: " + ex.Message);
				}
			}
			finally
			{
				((IDisposable)(object)saveFileDialog)?.Dispose();
			}
		}

		private static double GetTopElevationOfSlabBelow(List<Solid> allSlabSolids, double px, double py, double currentSlabMinZ, double defaultHeightFeet)
		{
			double num = double.MinValue;
			foreach (Solid allSlabSolid in allSlabSolids)
			{
				BoundingBoxXYZ solidBoundingBox = GetSolidBoundingBox(allSlabSolid);
				if (solidBoundingBox != null && solidBoundingBox.Max.Z < currentSlabMinZ - 0.1 && px >= solidBoundingBox.Min.X - 1.0 && px <= solidBoundingBox.Max.X + 1.0 && py >= solidBoundingBox.Min.Y - 1.0 && py <= solidBoundingBox.Max.Y + 1.0 && solidBoundingBox.Max.Z > num)
				{
					num = solidBoundingBox.Max.Z;
				}
			}
			if (num > double.MinValue)
			{
				return num;
			}
			return currentSlabMinZ - defaultHeightFeet;
		}

		private static int CreateSlabSupports(Document doc, Floor slab, List<Solid> slabSolids, List<Solid> allSlabSolids, List<Solid> allBeamSolids, List<Solid> allColumnSolids, List<Solid> allWallSolids, double globalMinSlabZ, Material timberMat, Material propMat, List<ShutterRecord> records, ShutteringSettings settings)
		{
			int num = 0;
			if (slabSolids == null || slabSolids.Count == 0)
			{
				return 0;
			}
			List<Solid> list = allBeamSolids.Concat(allColumnSolids).Concat(allWallSolids).ToList();
			double clearanceFeet = 125.0 / 762.0;
			foreach (Solid slabSolid in slabSolids)
			{
				BoundingBoxXYZ solidBoundingBox = GetSolidBoundingBox(slabSolid);
				if (solidBoundingBox == null)
				{
					continue;
				}
				double x = solidBoundingBox.Min.X;
				double x2 = solidBoundingBox.Max.X;
				double y = solidBoundingBox.Min.Y;
				double y2 = solidBoundingBox.Max.Y;
				double z = solidBoundingBox.Min.Z;
				double num2 = 100.0 / 381.0;
				double num3 = 250.0 / 381.0;
				double num4 = settings.PanelThicknessMm / 304.8;
				double num5 = z - num4;
				double num6 = num5 - num3;
				double num7 = settings.PrimaryJoistSpacingMm / 304.8;
				if (settings.ModelPrimaryJoists)
				{
					for (double num8 = y + num7 / 2.0; num8 <= y2 - num2 / 2.0; num8 += num7)
					{
						XYZ minPt = new XYZ(x, num8 - num2 / 2.0, num6);
						XYZ maxPt = new XYZ(x2, num8 + num2 / 2.0, num5);
						Solid solid = CreateBoxSolid(minPt, maxPt);
						if (solid == null)
						{
							continue;
						}
						if (list.Count > 0)
						{
							solid = SubtractSolids(solid, list);
						}
						if (solid != null && solid.Volume > 1E-05)
						{
							double a = (x2 - x) * 304.8;
							BuiltInCategory jCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
							ElementId elementId = CreateDirectShape(doc, solid, "Slab Primary Joist", $"Slab Primary H20 Joist - {Math.Round(a)}mm", timberMat, jCat);
							if (elementId != ElementId.InvalidElementId)
							{
								records.Add(new ShutterRecord
								{
									HostElementId = slab.Id.ToString(),
									HostCategory = slab.Category.Name,
									ShutterElementId = elementId.ToString(),
									ComponentType = "Primary Joist",
									ShutterMarkValue = $"Slab Primary H20 Joist - L={Math.Round(a)}mm",
									ShutterArea = 0.0,
									ElementWidth = 80.0,
									ElementHeight = 200.0,
									ElementLength = Math.Round(a),
									ElementThickness = 0.0
								});
								num++;
							}
						}
					}
				}
				double num9 = num6;
				double num10 = num9 - num3;
				double num11 = settings.SecondaryJoistSpacingMm / 304.8;
				if (settings.ModelSecondaryJoists)
				{
					for (double num12 = x + num11 / 2.0; num12 <= x2 - num2 / 2.0; num12 += num11)
					{
						XYZ minPt2 = new XYZ(num12 - num2 / 2.0, y, num10);
						XYZ maxPt2 = new XYZ(num12 + num2 / 2.0, y2, num9);
						Solid solid2 = CreateBoxSolid(minPt2, maxPt2);
						if (solid2 == null)
						{
							continue;
						}
						if (list.Count > 0)
						{
							solid2 = SubtractSolids(solid2, list);
						}
						if (solid2 != null && solid2.Volume > 1E-05)
						{
							double a2 = (y2 - y) * 304.8;
							BuiltInCategory jCat2 = settings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
							ElementId elementId2 = CreateDirectShape(doc, solid2, "Slab Secondary Joist", $"Slab Secondary H20 Joist - {Math.Round(a2)}mm", timberMat, jCat2);
							if (elementId2 != ElementId.InvalidElementId)
							{
								records.Add(new ShutterRecord
								{
									HostElementId = slab.Id.ToString(),
									HostCategory = slab.Category.Name,
									ShutterElementId = elementId2.ToString(),
									ComponentType = "Secondary Joist",
									ShutterMarkValue = $"Slab Secondary H20 Joist - L={Math.Round(a2)}mm",
									ShutterArea = 0.0,
									ElementWidth = 80.0,
									ElementHeight = 200.0,
									ElementLength = Math.Round(a2),
									ElementThickness = 0.0
								});
								num++;
							}
						}
					}
				}
				if (!settings.ModelProps)
				{
					continue;
				}
				if (settings.SkipGroundSlabProps && z <= globalMinSlabZ + 1.5)
				{
					continue;
				}
				double radiusFeet = 50.0 / 381.0;
				double num13 = num10;
				double num14 = settings.ScaffoldPropGridXmm / 304.8;
				double num15 = settings.ScaffoldPropGridYmm / 304.8;
				for (double num16 = x + num14 / 2.0; num16 <= x2; num16 += num14)
				{
					for (double num17 = y + num15 / 2.0; num17 <= y2; num17 += num15)
					{
						if (IsPointInsideConcrete(num16, num17, allColumnSolids, clearanceFeet))
						{
							continue;
						}
						double num18 = (settings.AutoDetectSlabBelow ? GetTopElevationOfSlabBelow(allSlabSolids, num16, num17, z, settings.DefaultPropHeightMm / 304.8) : (num13 - settings.DefaultPropHeightMm / 304.8));
						if (num18 < globalMinSlabZ)
						{
							num18 = globalMinSlabZ;
						}
						if (num13 <= num18 + 0.5)
						{
							continue;
						}
						XYZ topPt = new XYZ(num16, num17, num13);
						XYZ bottomPt = new XYZ(num16, num17, num18);
						Solid solid3 = CreateCylinderSolid(bottomPt, topPt, radiusFeet);
						if (solid3 != null && solid3.Volume > 1E-05)
						{
							double a3 = (num13 - num18) * 304.8;
							BuiltInCategory pCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
							ElementId elementId3 = CreateDirectShape(doc, solid3, "Slab Scaffold Prop", $"Slab Scaffolding Prop - H: {Math.Round(a3)}mm", propMat, pCat);
							if (elementId3 != ElementId.InvalidElementId)
							{
								records.Add(new ShutterRecord
								{
									HostElementId = slab.Id.ToString(),
									HostCategory = slab.Category.Name,
									ShutterElementId = elementId3.ToString(),
									ComponentType = "Scaffolding Prop",
									ShutterMarkValue = $"Slab Scaffolding Prop - H: {Math.Round(a3)}mm",
									ShutterArea = 0.0,
									ElementWidth = 80.0,
									ElementHeight = Math.Round(a3),
									ElementLength = 0.0,
									ElementThickness = 0.0
								});
								num++;
							}
						}
					}
				}
			}
			return num;
		}

		private static int CreateWallSupports(Document doc, Wall wall, List<Solid> wallSolids, Material steelWalerMat, Material propMat, Material tieRodMat, List<ShutterRecord> records, ShutteringSettings settings)
		{
			int num = 0;
			if (wallSolids == null || wallSolids.Count == 0)
			{
				return 0;
			}
			double num2 = settings.PanelThicknessMm / 304.8;
			double num3 = 125.0 / 381.0;
			double num4 = 125.0 / 381.0;
			double radiusFeet = 0.024606299212598423;
			double num5 = 0.39370078740157477;
			double num6 = 0.049212598425196846;
			foreach (Solid wallSolid in wallSolids)
			{
				BoundingBoxXYZ solidBoundingBox = GetSolidBoundingBox(wallSolid);
				if (solidBoundingBox == null)
				{
					continue;
				}
				foreach (Face face in wallSolid.Faces)
				{
					if (!(face is PlanarFace { FaceNormal: var faceNormal } planarFace) || Math.Abs(faceNormal.Z) >= 0.1)
					{
						continue;
					}
					XYZ xYZ = faceNormal.CrossProduct(XYZ.BasisZ).Normalize();
					List<XYZ> list = new List<XYZ>();
					foreach (EdgeArray edgeLoop in planarFace.EdgeLoops)
					{
						foreach (Edge item in edgeLoop)
						{
							IList<XYZ> list2 = item.Tessellate();
							if (list2 != null && list2.Count > 0)
							{
								list.AddRange(list2);
							}
						}
					}
					if (list.Count == 0)
					{
						continue;
					}
					XYZ xYZ2 = list[0];
					double num7 = double.MaxValue;
					double num8 = double.MinValue;
					double num9 = double.MaxValue;
					double num10 = double.MinValue;
					foreach (XYZ item2 in list)
					{
						double num11 = (item2 - xYZ2).DotProduct(xYZ);
						if (num11 < num7)
						{
							num7 = num11;
						}
						if (num11 > num8)
						{
							num8 = num11;
						}
						if (item2.Z < num9)
						{
							num9 = item2.Z;
						}
						if (item2.Z > num10)
						{
							num10 = item2.Z;
						}
					}
					double num12 = num8 - num7;
					if (num12 <= 0.0016404199475065617)
					{
						continue;
					}
					double num13 = settings.WalerVerticalSpacingMm / 304.8;
					for (double num14 = num9 + num13 / 2.0; num14 <= num10 - num4 / 2.0; num14 += num13)
					{
						XYZ xYZ3 = xYZ2 + num7 * xYZ;
						xYZ3 = new XYZ(xYZ3.X, xYZ3.Y, num14) + faceNormal * (num2 + num3 / 2.0);
						XYZ xYZ4 = xYZ2 + num8 * xYZ;
						xYZ4 = new XYZ(xYZ4.X, xYZ4.Y, num14) + faceNormal * (num2 + num3 / 2.0);
						if (settings.ModelWalers)
						{
							Solid solid = CreateBeamSolidBetweenPoints(xYZ3, xYZ4, num3, num4);
							if (solid != null && solid.Volume > 1E-05)
							{
								BuiltInCategory wCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
								ElementId elementId = CreateDirectShape(doc, solid, "Wall Waler", "Wall Steel Waler - 100x100mm", steelWalerMat, wCat);
								if (elementId != ElementId.InvalidElementId)
								{
									records.Add(new ShutterRecord
									{
										HostElementId = wall.Id.ToString(),
										HostCategory = wall.Category.Name,
										ShutterElementId = elementId.ToString(),
										ComponentType = "Waler",
										ShutterMarkValue = "Wall Steel Waler - 100x100mm",
										ShutterArea = 0.0,
										ElementWidth = 100.0,
										ElementHeight = 100.0,
										ElementLength = Math.Round(num12 * 304.8),
										ElementThickness = 0.0
									});
									num++;
								}
							}
						}
						if (!settings.GenerateTieRods)
						{
							continue;
						}
						double num15 = 3.9370078740157477;
						XYZ xYZ5 = (xYZ3 + xYZ4) * 0.5;
						for (double num16 = (0.0 - num12) / 2.0 + num15 / 2.0; num16 <= num12 / 2.0 - num15 / 2.0; num16 += num15)
						{
							XYZ xYZ6 = xYZ5 + xYZ * num16;
							double width = wall.WallType.Width;
							XYZ xYZ7 = xYZ6 + faceNormal * num6;
							XYZ xYZ8 = xYZ6 - faceNormal * (width + num6);
							Solid solid2 = CreateSlantedCylinderSolid(xYZ7, xYZ8, radiusFeet);
							if (solid2 != null && solid2.Volume > 1E-05)
							{
								ElementId elementId2 = CreateDirectShape(doc, solid2, "DOKA Dywidag Tie Rod", "DOKA Dywidag 15mm Tie Rod Bar", tieRodMat);
								if (elementId2 != ElementId.InvalidElementId)
								{
									records.Add(new ShutterRecord
									{
										HostElementId = wall.Id.ToString(),
										HostCategory = wall.Category.Name,
										ShutterElementId = elementId2.ToString(),
										ShutterMarkValue = "DOKA Dywidag 15mm Tie Rod Bar",
										ShutterArea = 0.0,
										ElementWidth = 15.0,
										ElementHeight = 15.0,
										ElementLength = Math.Round(xYZ7.DistanceTo(xYZ8) * 304.8),
										ElementThickness = 0.0
									});
									num++;
								}
							}
							XYZ xYZ9 = xYZ6 + faceNormal * (num2 + num3 + num6 / 2.0);
							XYZ minPt = xYZ9 - xYZ * (num5 / 2.0) - XYZ.BasisZ * (num5 / 2.0) - faceNormal * (num6 / 2.0);
							XYZ maxPt = xYZ9 + xYZ * (num5 / 2.0) + XYZ.BasisZ * (num5 / 2.0) + faceNormal * (num6 / 2.0);
							Solid solid3 = CreateBoxSolid(minPt, maxPt);
							if (solid3 != null && solid3.Volume > 1E-05)
							{
								ElementId elementId3 = CreateDirectShape(doc, solid3, "DOKA Wing Nut Plate", "DOKA Wing Nut Anchor Plate 120x120mm", steelWalerMat);
								if (elementId3 != ElementId.InvalidElementId)
								{
									num++;
								}
							}
						}
					}
					double num17 = num10 - num9;
					if (num17 < 0.000984251968503937)
					{
						continue;
					}
					double num18 = 50.0 / 381.0;
					double num19 = 3.9370078740157477;
					double num20 = 250.0 / 381.0;
					double num21 = 250.0 / 381.0;
					double num22 = 0.049212598425196846;
					double num23 = 0.667;
					double z = num9 + num17 * num23;
					double num24 = num17 * 0.55;
					int num25 = Math.Max(1, (int)Math.Floor(num12 / num19));
					double num26 = num12 / (double)num25;
					if (settings.ModelProps)
					{
						for (int i = 0; i < num25; i++)
						{
							double num27 = num7 + ((double)i + 0.5) * num26;
							XYZ xYZ10 = xYZ2 + num27 * xYZ;
							XYZ xYZ11 = new XYZ(xYZ10.X, xYZ10.Y, z) + faceNormal * (num2 + num3);
							XYZ xYZ12 = new XYZ(xYZ10.X, xYZ10.Y, num9) + faceNormal * (num2 + num3 + num24);
							Solid solid4 = CreateSlantedCylinderSolid(xYZ12, xYZ11, num18);
							if (solid4 != null && solid4.Volume > 1E-05)
							{
								double a = xYZ12.DistanceTo(xYZ11) * 304.8;
								BuiltInCategory prCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
								ElementId elementId4 = CreateDirectShape(doc, solid4, "Wall Push-Pull Prop", $"Wall Push-Pull Diagonal Prop - L={Math.Round(a)}mm", propMat, prCat);
								if (elementId4 != ElementId.InvalidElementId)
								{
									records.Add(new ShutterRecord
									{
										HostElementId = wall.Id.ToString(),
										HostCategory = wall.Category.Name,
										ShutterElementId = elementId4.ToString(),
										ComponentType = "Push-Pull Prop",
										ShutterMarkValue = $"Wall Push-Pull Diagonal Prop - L={Math.Round(a)}mm",
										ShutterArea = 0.0,
										ElementWidth = 80.0,
										ElementHeight = Math.Round(a),
										ElementLength = 0.0,
										ElementThickness = 0.0
									});
									num++;
								}
							}
							XYZ xYZ13 = xYZ12;
							XYZ xYZ14 = new XYZ(xYZ13.X - num20 / 2.0, xYZ13.Y - num21 / 2.0, num9);
							XYZ xYZ15 = new XYZ(xYZ13.X + num20 / 2.0, xYZ13.Y + num21 / 2.0, num9 + num22);
							XYZ xYZ16 = (xYZ14 + xYZ15) * 0.5;
							XYZ xYZ17 = xYZ * (num20 / 2.0);
							XYZ xYZ18 = faceNormal * (num21 / 2.0);
							XYZ xYZ19 = XYZ.BasisZ * (num22 / 2.0);
							XYZ minPt2 = xYZ16 - xYZ17 - xYZ18 - xYZ19;
							XYZ maxPt2 = xYZ16 + xYZ17 + xYZ18 + xYZ19;
							Solid solid5 = CreateBoxSolid(minPt2, maxPt2);
							if (solid5 != null && solid5.Volume > 1E-05)
							{
								BuiltInCategory prCat2 = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
								ElementId elementId5 = CreateDirectShape(doc, solid5, "Wall Push-Pull Prop", "Prop Base Kicker Plate 200x200x15mm", steelWalerMat, prCat2);
								if (elementId5 != ElementId.InvalidElementId)
								{
									num++;
								}
							}
							double num28 = 100.0 / 381.0;
							XYZ startPt = new XYZ(xYZ12.X, xYZ12.Y, num9 + num22);
							XYZ endPt = new XYZ(xYZ12.X, xYZ12.Y, num9 + num22 + num28);
							Solid solid6 = CreateSlantedCylinderSolid(startPt, endPt, num18 * 0.6);
							if (solid6 != null && solid6.Volume > 1E-05)
							{
								BuiltInCategory prCat3 = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
								ElementId elementId6 = CreateDirectShape(doc, solid6, "Wall Push-Pull Prop", "Prop Base Foot Connector", propMat, prCat3);
								if (elementId6 != ElementId.InvalidElementId)
								{
									num++;
								}
							}
						}
					}
				}
			}
			return num;
		}

		private static int CreateColumnSupports(Document doc, FamilyInstance col, List<Solid> colSolids, Material steelWalerMat, Material propMat, Material tieRodMat, List<ShutterRecord> records, ShutteringSettings settings)
		{
			int num = 0;
			if (colSolids == null || colSolids.Count == 0)
			{
				return 0;
			}
			double num2 = settings.PanelThicknessMm / 304.8;
			double num3 = 100.0 / 381.0;
			double num4 = 100.0 / 381.0;
			foreach (Solid colSolid in colSolids)
			{
				BoundingBoxXYZ solidBoundingBox = GetSolidBoundingBox(colSolid);
				if (solidBoundingBox == null)
				{
					continue;
				}
				double z = solidBoundingBox.Min.Z;
				double z2 = solidBoundingBox.Max.Z;
				double num5 = settings.WalerVerticalSpacingMm / 304.8;
				for (double num6 = z + num5 / 2.0; num6 <= z2 - num4; num6 += num5)
				{
					if (settings.ModelWalers)
					{
						XYZ minPt = new XYZ(solidBoundingBox.Min.X - num2 - num3, solidBoundingBox.Min.Y - num2 - num3, num6);
						XYZ maxPt = new XYZ(solidBoundingBox.Max.X + num2 + num3, solidBoundingBox.Max.Y + num2 + num3, num6 + num4);
						Solid solid = CreateBoxSolid(minPt, maxPt);
						XYZ minPt2 = new XYZ(solidBoundingBox.Min.X - num2, solidBoundingBox.Min.Y - num2, num6 - 0.1);
						XYZ maxPt2 = new XYZ(solidBoundingBox.Max.X + num2, solidBoundingBox.Max.Y + num2, num6 + num4 + 0.1);
						Solid solid2 = CreateBoxSolid(minPt2, maxPt2);
						if (solid != null && solid2 != null)
						{
							solid = SubtractSolids(solid, new List<Solid> { solid2 });
						}
						if (solid != null && solid.Volume > 1E-05)
						{
							BuiltInCategory cCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
							ElementId elementId = CreateDirectShape(doc, solid, "Column Waler", "Column Clamping Waler Frame", steelWalerMat, cCat);
							if (elementId != ElementId.InvalidElementId)
							{
								records.Add(new ShutterRecord
								{
									HostElementId = col.Id.ToString(),
									HostCategory = col.Category.Name,
									ShutterElementId = elementId.ToString(),
									ComponentType = "Waler",
									ShutterMarkValue = "Column Clamping Waler Frame",
									ShutterArea = 0.0,
									ElementWidth = 80.0,
									ElementHeight = 80.0,
									ElementLength = 0.0,
									ElementThickness = 0.0
								});
								num++;
							}
						}
					}
				}
				if (settings.ModelProps)
				{
					XYZ xYZ = new XYZ(solidBoundingBox.Max.X + num2 + num3, (solidBoundingBox.Min.Y + solidBoundingBox.Max.Y) / 2.0, z + (z2 - z) * 0.7);
					XYZ xYZ2 = xYZ + new XYZ((z2 - z) * 0.5, 0.0, (0.0 - (z2 - z)) * 0.7);
					Solid solid3 = CreateSlantedCylinderSolid(xYZ2, xYZ, 50.0 / 381.0);
					if (solid3 != null && solid3.Volume > 1E-05)
					{
						double a = xYZ2.DistanceTo(xYZ) * 304.8;
						BuiltInCategory cpCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
						ElementId elementId2 = CreateDirectShape(doc, solid3, "Column Support Prop", $"Column Push-Pull Prop - L={Math.Round(a)}mm", propMat, cpCat);
						if (elementId2 != ElementId.InvalidElementId)
						{
							records.Add(new ShutterRecord
							{
								HostElementId = col.Id.ToString(),
								HostCategory = col.Category.Name,
								ShutterElementId = elementId2.ToString(),
								ComponentType = "Push-Pull Prop",
								ShutterMarkValue = $"Column Push-Pull Prop - L={Math.Round(a)}mm",
								ShutterArea = 0.0,
								ElementWidth = 80.0,
								ElementHeight = Math.Round(a),
								ElementLength = 0.0,
								ElementThickness = 0.0
							});
							num++;
						}
					}
				}
			}
			return num;
		}

		private static int CreateBeamSupports(Document doc, FamilyInstance beam, List<Solid> beamSolids, List<Solid> allSlabSolids, List<Solid> allColumnSolids, List<Solid> allWallSolids, double globalMinSlabZ, Material steelWalerMat, Material propMat, List<ShutterRecord> records, ShutteringSettings settings)
		{
			int num = 0;
			if (beamSolids == null || beamSolids.Count == 0)
			{
				return 0;
			}
			List<Solid> list = allColumnSolids.Concat(allWallSolids).ToList();
			double clearanceFeet = 25.0 / 762.0;
			double num2 = settings.PanelThicknessMm / 304.8;
			double num3 = 100.0 / 381.0;
			double num4 = 125.0 / 381.0;
			double radiusFeet = 50.0 / 381.0;
			double num5 = 2.6246719160104988;
			foreach (Solid beamSolid in beamSolids)
			{
				BoundingBoxXYZ solidBoundingBox = GetSolidBoundingBox(beamSolid);
				if (solidBoundingBox == null)
				{
					continue;
				}
				double x = solidBoundingBox.Min.X;
				double x2 = solidBoundingBox.Max.X;
				double y = solidBoundingBox.Min.Y;
				double y2 = solidBoundingBox.Max.Y;
				double z = solidBoundingBox.Min.Z;
				double num6 = z - num2;
				double num7 = num6 - num4;
				if (x2 - x >= y2 - y)
				{
					for (double num8 = x + num5 / 2.0; num8 <= x2 - num3 / 2.0; num8 += num5)
					{
						if (settings.ModelWalers)
						{
							XYZ minPt = new XYZ(num8 - num3 / 2.0, y - 125.0 / 381.0, num7);
							XYZ maxPt = new XYZ(num8 + num3 / 2.0, y2 + 125.0 / 381.0, num6);
							Solid solid = CreateBoxSolid(minPt, maxPt);
							if (solid != null && list.Count > 0)
							{
								solid = SubtractSolids(solid, list);
							}
							if (solid != null && solid.Volume > 1E-05)
							{
								BuiltInCategory bwCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
								ElementId elementId = CreateDirectShape(doc, solid, "Beam Bottom Support", "Beam Bottom Transverse Support Waler", steelWalerMat, bwCat);
								if (elementId != ElementId.InvalidElementId)
								{
									records.Add(new ShutterRecord
									{
										HostElementId = beam.Id.ToString(),
										HostCategory = beam.Category.Name,
										ShutterElementId = elementId.ToString(),
										ComponentType = "Waler",
										ShutterMarkValue = "Beam Bottom Transverse Support Waler",
										ShutterArea = 0.0,
										ElementWidth = 80.0,
										ElementHeight = 100.0,
										ElementLength = Math.Round((y2 - y) * 304.8 + 200.0),
										ElementThickness = 0.0
									});
									num++;
								}
							}
						}
						if (!settings.ModelProps)
						{
							continue;
						}
						if (settings.SkipGroundSlabProps && z <= globalMinSlabZ + 1.5)
						{
							continue;
						}
						double num9 = num8;
						double num10 = (y + y2) / 2.0;
						if (IsPointInsideConcrete(num9, num10, list, clearanceFeet))
						{
							continue;
						}
						double num11 = num7;
						double num12 = (settings.AutoDetectSlabBelow ? GetTopElevationOfSlabBelow(allSlabSolids, num9, num10, z, settings.DefaultPropHeightMm / 304.8) : (num11 - settings.DefaultPropHeightMm / 304.8));
						if (num12 < globalMinSlabZ)
						{
							num12 = globalMinSlabZ;
						}
						if (num11 <= num12 + 0.5)
						{
							continue;
						}
						XYZ topPt = new XYZ(num9, num10, num11);
						XYZ bottomPt = new XYZ(num9, num10, num12);
						Solid solid2 = CreateCylinderSolid(bottomPt, topPt, radiusFeet);
						if (solid2 != null && solid2.Volume > 1E-05)
						{
							double a = (num11 - num12) * 304.8;
							BuiltInCategory bpCat = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
							ElementId elementId2 = CreateDirectShape(doc, solid2, "Beam Support Prop", $"Beam Shoring Prop - H: {Math.Round(a)}mm", propMat, bpCat);
							if (elementId2 != ElementId.InvalidElementId)
							{
								records.Add(new ShutterRecord
								{
									HostElementId = beam.Id.ToString(),
									HostCategory = beam.Category.Name,
									ShutterElementId = elementId2.ToString(),
									ComponentType = "Scaffolding Prop",
									ShutterMarkValue = $"Beam Shoring Prop - H: {Math.Round(a)}mm",
									ShutterArea = 0.0,
									ElementWidth = 80.0,
									ElementHeight = Math.Round(a),
									ElementLength = 0.0,
									ElementThickness = 0.0
								});
								num++;
							}
						}
					}
					continue;
				}
				for (double num13 = y + num5 / 2.0; num13 <= y2 - num3 / 2.0; num13 += num5)
				{
					if (settings.ModelWalers)
					{
						XYZ minPt2 = new XYZ(x - 125.0 / 381.0, num13 - num3 / 2.0, num7);
						XYZ maxPt2 = new XYZ(x2 + 125.0 / 381.0, num13 + num3 / 2.0, num6);
						Solid solid3 = CreateBoxSolid(minPt2, maxPt2);
						if (solid3 != null && list.Count > 0)
						{
							solid3 = SubtractSolids(solid3, list);
						}
						if (solid3 != null && solid3.Volume > 1E-05)
						{
							BuiltInCategory bwCat2 = settings.SeparateCategories ? BuiltInCategory.OST_StructuralFraming : BuiltInCategory.OST_GenericModel;
							ElementId elementId3 = CreateDirectShape(doc, solid3, "Beam Bottom Support", "Beam Bottom Transverse Support Waler", steelWalerMat, bwCat2);
							if (elementId3 != ElementId.InvalidElementId)
							{
								records.Add(new ShutterRecord
								{
									HostElementId = beam.Id.ToString(),
									HostCategory = beam.Category.Name,
									ShutterElementId = elementId3.ToString(),
									ComponentType = "Waler",
									ShutterMarkValue = "Beam Bottom Transverse Support Waler",
									ShutterArea = 0.0,
									ElementWidth = 80.0,
									ElementHeight = 100.0,
									ElementLength = Math.Round((x2 - x) * 304.8 + 200.0),
									ElementThickness = 0.0
								});
								num++;
							}
						}
					}
					if (!settings.ModelProps)
					{
						continue;
					}
					if (settings.SkipGroundSlabProps && z <= globalMinSlabZ + 1.5)
					{
						continue;
					}
					double num14 = (x + x2) / 2.0;
					double num15 = num13;
					if (IsPointInsideConcrete(num14, num15, list, clearanceFeet))
					{
						continue;
					}
					double num16 = num7;
					double num17 = (settings.AutoDetectSlabBelow ? GetTopElevationOfSlabBelow(allSlabSolids, num14, num15, z, settings.DefaultPropHeightMm / 304.8) : (num16 - settings.DefaultPropHeightMm / 304.8));
					if (num17 < globalMinSlabZ)
					{
						num17 = globalMinSlabZ;
					}
					if (num16 <= num17 + 0.5)
					{
						continue;
					}
					XYZ topPt2 = new XYZ(num14, num15, num16);
					XYZ bottomPt2 = new XYZ(num14, num15, num17);
					Solid solid4 = CreateCylinderSolid(bottomPt2, topPt2, radiusFeet);
					if (solid4 != null && solid4.Volume > 1E-05)
					{
						double a2 = (num16 - num17) * 304.8;
						BuiltInCategory bpCat2 = settings.SeparateCategories ? BuiltInCategory.OST_StructuralColumns : BuiltInCategory.OST_GenericModel;
						ElementId elementId4 = CreateDirectShape(doc, solid4, "Beam Support Prop", $"Beam Shoring Prop - H: {Math.Round(a2)}mm", propMat, bpCat2);
						if (elementId4 != ElementId.InvalidElementId)
						{
							records.Add(new ShutterRecord
							{
								HostElementId = beam.Id.ToString(),
								HostCategory = beam.Category.Name,
								ShutterElementId = elementId4.ToString(),
								ComponentType = "Scaffolding Prop",
								ShutterMarkValue = $"Beam Shoring Prop - H: {Math.Round(a2)}mm",
								ShutterArea = 0.0,
								ElementWidth = 80.0,
								ElementHeight = Math.Round(a2),
								ElementLength = 0.0,
								ElementThickness = 0.0
							});
							num++;
						}
					}
				}
			}
			return num;
		}

		private static bool IsPointInsideConcrete(double px, double py, List<Solid> concreteSolids, double clearanceFeet)
		{
			foreach (Solid concreteSolid in concreteSolids)
			{
				BoundingBoxXYZ solidBoundingBox = GetSolidBoundingBox(concreteSolid);
				if (solidBoundingBox == null || !(px >= solidBoundingBox.Min.X - clearanceFeet) || !(px <= solidBoundingBox.Max.X + clearanceFeet) || !(py >= solidBoundingBox.Min.Y - clearanceFeet) || !(py <= solidBoundingBox.Max.Y + clearanceFeet))
				{
					continue;
				}
				return true;
			}
			return false;
		}

		private static BoundingBoxXYZ GetSolidBoundingBox(Solid solid)
		{
			try
			{
				double num = double.MaxValue;
				double num2 = double.MaxValue;
				double num3 = double.MaxValue;
				double num4 = double.MinValue;
				double num5 = double.MinValue;
				double num6 = double.MinValue;
				foreach (Edge edge in solid.Edges)
				{
					IList<XYZ> list = edge.Tessellate();
					foreach (XYZ item in list)
					{
						if (item.X < num)
						{
							num = item.X;
						}
						if (item.Y < num2)
						{
							num2 = item.Y;
						}
						if (item.Z < num3)
						{
							num3 = item.Z;
						}
						if (item.X > num4)
						{
							num4 = item.X;
						}
						if (item.Y > num5)
						{
							num5 = item.Y;
						}
						if (item.Z > num6)
						{
							num6 = item.Z;
						}
					}
				}
				BoundingBoxXYZ boundingBoxXYZ = new BoundingBoxXYZ();
				boundingBoxXYZ.Min = new XYZ(num, num2, num3);
				boundingBoxXYZ.Max = new XYZ(num4, num5, num6);
				return boundingBoxXYZ;
			}
			catch
			{
				return null;
			}
		}

		private static Solid CreateBoxSolid(XYZ minPt, XYZ maxPt)
		{
			try
			{
				double num = maxPt.X - minPt.X;
				double num2 = maxPt.Y - minPt.Y;
				double num3 = maxPt.Z - minPt.Z;
				if (num <= 0.001 || num2 <= 0.001 || num3 <= 0.001)
				{
					return null;
				}
				CurveLoop curveLoop = new CurveLoop();
				XYZ xYZ = new XYZ(minPt.X, minPt.Y, minPt.Z);
				XYZ xYZ2 = new XYZ(maxPt.X, minPt.Y, minPt.Z);
				XYZ xYZ3 = new XYZ(maxPt.X, maxPt.Y, minPt.Z);
				XYZ xYZ4 = new XYZ(minPt.X, maxPt.Y, minPt.Z);
				curveLoop.Append(Line.CreateBound(xYZ, xYZ2));
				curveLoop.Append(Line.CreateBound(xYZ2, xYZ3));
				curveLoop.Append(Line.CreateBound(xYZ3, xYZ4));
				curveLoop.Append(Line.CreateBound(xYZ4, xYZ));
				return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, XYZ.BasisZ, num3);
			}
			catch
			{
				return null;
			}
		}

		private static Solid CreateCylinderSolid(XYZ bottomPt, XYZ topPt, double radiusFeet)
		{
			try
			{
				double num = topPt.Z - bottomPt.Z;
				if (num <= 0.001 || radiusFeet <= 0.001)
				{
					return null;
				}
				Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, bottomPt);
				Arc curve = Arc.Create(plane, radiusFeet, 0.0, Math.PI);
				Arc curve2 = Arc.Create(plane, radiusFeet, Math.PI, Math.PI * 2.0);
				CurveLoop curveLoop = new CurveLoop();
				curveLoop.Append(curve);
				curveLoop.Append(curve2);
				return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, XYZ.BasisZ, num);
			}
			catch
			{
				return null;
			}
		}

		private static Solid CreateSlantedCylinderSolid(XYZ startPt, XYZ endPt, double radiusFeet)
		{
			try
			{
				XYZ xYZ = endPt - startPt;
				double length = xYZ.GetLength();
				if (length <= 0.01 || radiusFeet <= 0.001)
				{
					return null;
				}
				XYZ xYZ2 = xYZ.Normalize();
				XYZ source = ((Math.Abs(xYZ2.Z) < 0.9) ? XYZ.BasisZ : XYZ.BasisX);
				XYZ xYZ3 = xYZ2.CrossProduct(source).Normalize();
				XYZ basisY = xYZ2.CrossProduct(xYZ3).Normalize();
				Plane plane = Plane.CreateByOriginAndBasis(startPt, xYZ3, basisY);
				Arc curve = Arc.Create(plane, radiusFeet, 0.0, Math.PI);
				Arc curve2 = Arc.Create(plane, radiusFeet, Math.PI, Math.PI * 2.0);
				CurveLoop curveLoop = new CurveLoop();
				curveLoop.Append(curve);
				curveLoop.Append(curve2);
				return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, xYZ2, length);
			}
			catch
			{
				return null;
			}
		}

		private static Solid CreateBeamSolidBetweenPoints(XYZ p1, XYZ p2, double widthFeet, double heightFeet)
		{
			try
			{
				XYZ xYZ = p2 - p1;
				double length = xYZ.GetLength();
				if (length <= 0.01)
				{
					return null;
				}
				XYZ xYZ2 = xYZ.Normalize();
				XYZ source = ((Math.Abs(xYZ2.Z) < 0.9) ? XYZ.BasisZ : XYZ.BasisX);
				XYZ xYZ3 = xYZ2.CrossProduct(source).Normalize();
				XYZ xYZ4 = xYZ2.CrossProduct(xYZ3).Normalize();
				XYZ xYZ5 = xYZ3 * (widthFeet / 2.0);
				XYZ xYZ6 = xYZ4 * (heightFeet / 2.0);
				XYZ xYZ7 = p1 - xYZ5 - xYZ6;
				XYZ xYZ8 = p1 + xYZ5 - xYZ6;
				XYZ xYZ9 = p1 + xYZ5 + xYZ6;
				XYZ xYZ10 = p1 - xYZ5 + xYZ6;
				CurveLoop curveLoop = new CurveLoop();
				curveLoop.Append(Line.CreateBound(xYZ7, xYZ8));
				curveLoop.Append(Line.CreateBound(xYZ8, xYZ9));
				curveLoop.Append(Line.CreateBound(xYZ9, xYZ10));
				curveLoop.Append(Line.CreateBound(xYZ10, xYZ7));
				return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, xYZ2, length);
			}
			catch
			{
				return null;
			}
		}

		private static Material GetOrCreateMaterial(Document doc, string name, Color color)
		{
			Material material = new FilteredElementCollector(doc).OfClass(typeof(Material)).Cast<Material>().FirstOrDefault((Material m) => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			if (material == null)
			{
				ElementId id = Material.Create(doc, name);
				material = doc.GetElement(id) as Material;
				if (material != null)
				{
					material.Color = color;
				}
			}
			return material;
		}
	}
}
