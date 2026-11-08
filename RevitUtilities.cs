// RevitUtilities.cs – collection of helper functions for Revit add-in
// Supports Revit 2020 … 2027 via conditional compilation symbols (REVIT2020 … REVIT2027)

using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace RevitShutteringAddin
{
    public static class RevitUtilities
    {
        // ---------------------------------------------------------------------
        // Geometry helpers
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the bounding box of a solid, handling null gracefully.
        /// </summary>
        public static BoundingBoxXYZ GetSolidBoundingBox(Solid solid)
        {
            if (solid == null) return null;
            try { return solid.GetBoundingBox(); } catch { return null; }
        }

        /// <summary>
        /// Creates a rectangular box solid from two opposite corner points.
        /// </summary>
        public static Solid CreateBoxSolid(XYZ min, XYZ max)
        {
            try
            {
                XYZ p0 = new XYZ(min.X, min.Y, min.Z);
                XYZ p1 = new XYZ(max.X, min.Y, min.Z);
                XYZ p2 = new XYZ(max.X, max.Y, min.Z);
                XYZ p3 = new XYZ(min.X, max.Y, min.Z);

                CurveLoop loop = new CurveLoop();
                loop.Append(Line.CreateBound(p0, p1));
                loop.Append(Line.CreateBound(p1, p2));
                loop.Append(Line.CreateBound(p2, p3));
                loop.Append(Line.CreateBound(p3, p0));

                double height = max.Z - min.Z;
                if (height <= 0.0001) height = 0.001;

                return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { loop }, XYZ.BasisZ, height);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a beam-like solid between two points with given width and height.
        /// </summary>
        public static Solid CreateBeamSolidBetweenPoints(XYZ p1, XYZ p2, double width, double height)
        {
            try
            {
                XYZ dir = (p2 - p1).Normalize();
                XYZ up = XYZ.BasisZ;
                XYZ side = dir.CrossProduct(up);
                if (side.GetLength() < 0.001)
                {
                    side = dir.CrossProduct(XYZ.BasisX).Normalize();
                }
                else
                {
                    side = side.Normalize();
                }
                up = side.CrossProduct(dir).Normalize();

                double halfW = width / 2.0;
                double halfH = height / 2.0;

                XYZ c1 = p1 - side * halfW - up * halfH;
                XYZ c2 = p1 + side * halfW - up * halfH;
                XYZ c3 = p1 + side * halfW + up * halfH;
                XYZ c4 = p1 - side * halfW + up * halfH;

                CurveLoop loop = new CurveLoop();
                loop.Append(Line.CreateBound(c1, c2));
                loop.Append(Line.CreateBound(c2, c3));
                loop.Append(Line.CreateBound(c3, c4));
                loop.Append(Line.CreateBound(c4, c1));

                double length = p1.DistanceTo(p2);
                if (length <= 0.0001) return null;

                return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { loop }, dir, length);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a slanted cylindrical solid (e.g., a tie-rod) between two points.
        /// </summary>
        public static Solid CreateSlantedCylinderSolid(XYZ start, XYZ end, double radius)
        {
            try
            {
                XYZ dir = (end - start).Normalize();
                double height = start.DistanceTo(end);
                if (height <= 0.0001) return null;

                XYZ normal = dir;
                XYZ refV = Math.Abs(normal.Z) < 0.9 ? XYZ.BasisZ : XYZ.BasisX;
                XYZ v1 = normal.CrossProduct(refV).Normalize();
                XYZ v2 = normal.CrossProduct(v1).Normalize();

                Plane plane = Plane.CreateByNormalAndOrigin(normal, start);
                Arc arc1 = Arc.Create(plane, radius, 0, Math.PI);
                Arc arc2 = Arc.Create(plane, radius, Math.PI, 2 * Math.PI);

                CurveLoop loop = new CurveLoop();
                loop.Append(arc1);
                loop.Append(arc2);

                return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { loop }, normal, height);
            }
            catch
            {
                return null;
            }
        }

        // ---------------------------------------------------------------------
        // DirectShape helper – wraps the Revit API call with material assignment.
        // ---------------------------------------------------------------------
        public static ElementId CreateDirectShape(Document doc, Solid solid, string name, string displayName, Material material)
        {
            if (solid == null) return ElementId.InvalidElementId;
            try
            {
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new GeometryObject[] { solid });
                ds.Name = name;
                if (material != null)
                {
                    ds.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM)?.Set(material.Id);
                }
                return ds.Id;
            }
            catch
            {
                return ElementId.InvalidElementId;
            }
        }

        // ---------------------------------------------------------------------
        // Revit version detection – returns the major version number (e.g., 2020).
        // ---------------------------------------------------------------------
        public static int GetRevitYear(Autodesk.Revit.ApplicationServices.Application app)
        {
            if (app != null && int.TryParse(app.VersionNumber, out int year))
                return year;
            return 2020;
        }

        /// <summary>
        /// Gets the integer/long ID value safely across Revit 2020 through 2027.
        /// Avoids MissingMethodException on get_IntegerValue() in Revit 2024+.
        /// </summary>
        public static long GetIdValue(ElementId id)
        {
            if (id == null) return -1;
            try
            {
                // In Revit 2024+, ElementId.Value returns Int64 (long)
                System.Reflection.PropertyInfo propValue = typeof(ElementId).GetProperty("Value");
                if (propValue != null)
                {
                    return Convert.ToInt64(propValue.GetValue(id));
                }
                // In Revit 2020-2023, ElementId.IntegerValue returns Int32 (int)
                System.Reflection.PropertyInfo propInt = typeof(ElementId).GetProperty("IntegerValue");
                if (propInt != null)
                {
                    return Convert.ToInt64(propInt.GetValue(id));
                }
            }
            catch
            {
            }
            // Fallback via string parse
            if (long.TryParse(id.ToString(), out long val))
                return val;
            return -1;
        }
    }
}
