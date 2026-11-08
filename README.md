# BimTools.Shuttering

**Advanced 3D Formwork & Shuttering Modeling Engine for Autodesk® Revit®**  
*Supporting Autodesk Revit 2020, 2021, 2022, 2023, 2024, 2025, 2026, and 2027.*

---

## 🏗 Overview

**BimTools.Shuttering** is an automated formwork and scaffolding generation add-in for Autodesk Revit. It creates detailed 3D DirectShape formwork panels, walers, tie rods, props, and scaffolding systems for structural concrete elements:
* **Slabs** (Soffit / underside & perimeter side formwork)
* **Beams** (Bottom soffit & side vertical formwork, subtracting column overlaps)
* **Columns** (Perimeter formwork panels & clamping walers)
* **Walls / Core Walls** (Double-sided formwork with edge-tessellated walers and push-pull props)

---

## ✨ Key Features

* **Advanced DOKA / PERI / Universal System Modeling**:
  * **H20 Timber Joists**: Primary and secondary cross-joist grids under slabs.
  * **Dywidag 15mm Tie Rods**: Complete with 120×120mm DOKA wing nut anchor plates.
  * **Edge-Tessellated Wall Walers**: Walers span precisely across wall face extents without overshooting or displacement.
  * **Parapet & Wall Push-Pull Props**: Dynamic diagonal props spaced every 1200mm, each featuring a 200×200×15mm base kicker plate sitting flat on the slab and a vertical foot connector stub.
  * **Elevation & Ground Slab Detection**: Automatically detects lower slab tops to extend shoring props, with an option to skip props under ground-level slabs resting on grade.
* **Full Multi-Version Compatibility (Revit 2020 – 2027)**:
  * Seamless cross-version API resolution (including the `ElementId.IntegerValue` to `ElementId.Value` transition in Revit 2024+).
  * Standalone compiled DLLs available for every Revit release from 2020 through 2027.
* **Automated CSV Quantity Scheduling**:
  * Exports comprehensive formwork schedules with host element IDs, categories, dimensions (W × H × L × T), shutter element IDs, and calculated surface areas ($m^2$).

---

## 📁 Repository Structure

```text
BimTools.Shuttering/
│
├── App.cs                          # IExternalApplication ribbon UI registration (BIM TOOLS tab)
├── Command.cs                      # Core shuttering and formwork generation command
├── SettingsForm.cs                 # WinForms configuration dialog
├── ShutteringSettings.cs           # Settings model
├── ShutterRecord.cs                # Formwork schedule data structure
├── WindowHandle.cs                 # Revit window owner handle wrapper
├── RevitUtilities.cs               # Cross-version geometry and API helper library
├── RevitShutteringAddin.csproj     # C# project file (targeting .NET 4.8, x64)
├── RevitShutteringAddin.sln        # Visual Studio solution file
├── RevitShutteringAddin.addin      # Revit Add-in manifest file
│
├── RevitShutteringAddin_2020.dll   # Precompiled binary for Revit 2020
├── RevitShutteringAddin_2021.dll   # Precompiled binary for Revit 2021
├── RevitShutteringAddin_2022.dll   # Precompiled binary for Revit 2022
├── RevitShutteringAddin_2023.dll   # Precompiled binary for Revit 2023
├── RevitShutteringAddin_2024.dll   # Precompiled binary for Revit 2024
├── RevitShutteringAddin_2025.dll   # Precompiled binary for Revit 2025
├── RevitShutteringAddin_2026.dll   # Precompiled binary for Revit 2026
└── RevitShutteringAddin_2027.dll   # Precompiled binary for Revit 2027
```

---

## 🚀 Installation & Deployment

1. Copy the DLL corresponding to your Revit version into:
   ```text
   %APPDATA%\Autodesk\Revit\Addins\<Year>\RevitShutteringAddin.dll
   ```
2. Copy `RevitShutteringAddin.addin` to:
   ```text
   %APPDATA%\Autodesk\Revit\Addins\<Year>\RevitShutteringAddin.addin
   ```
3. Ensure the `<Assembly>` path inside the `.addin` file matches the DLL location:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <RevitAddIns>
     <AddIn Type="Application">
       <Name>Shuttering Addin</Name>
       <Assembly>C:\Users\<YourUser>\AppData\Roaming\Autodesk\Revit\Addins\2020\RevitShutteringAddin.dll</Assembly>
       <FullClassName>RevitShutteringAddin.App</FullClassName>
       <ClientId>a3b5c7d9-1e2f-48d0-9a2b-8c6d4e5f7a2d</ClientId>
       <VendorId>Amdghedy</VendorId>
     </AddIn>
   </RevitAddIns>
   ```
4. Start Revit — the **Shuttering** button will appear under the **BIM TOOLS** tab.

---

## 🛠️ Building from Source

To build for any specific Revit version:

```powershell
dotnet build RevitShutteringAddin.csproj -c Release -p:DefineConstants=REVIT2024 -p:RevitApiPath="C:\Program Files\Autodesk\Revit 2024"
```

---

## 👤 Author

**Eng. Abdelraheem Mohamed Deghedy**  
* Email: [amdghedy@gmail.com](mailto:amdghedy@gmail.com)  
* Mobile: [+201001877018](tel:+201001877018)  
* GitHub: [@amdghedy](https://github.com/amdghedy)
