﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMiconToolbar.MatchGrids
{
    [TransactionAttribute(TransactionMode.Manual)]
    class MatchGrids : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            using (MatchGridsWPF customWindow = new MatchGridsWPF(commandData))
            {
                customWindow.ShowDialog();
            }

            return Result.Succeeded;
        }
    }
}
