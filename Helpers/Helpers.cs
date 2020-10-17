﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BIMiconToolbar.Helpers
{
    class Helpers
    {
        /// <summary>
        /// A function to read and store content of a file even if it is in use
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string WriteSafeReadAllLines(string path)
        {
            using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(csv))
            {
                List<string> file = new List<string>();
                while (!sr.EndOfStream)
                {
                    file.Add(sr.ReadLine());
                }

                return String.Join("", file.ToArray());
            }
        }

        /// <summary>
        /// A function to number Family Instances
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="builtInCategory"></param>
        public static void numberFamilyInstance(Document doc,
                                                BuiltInCategory builtInCategory)
        {
            // Create dictionary to store window-room values
            Dictionary<FamilyInstance, string> instanceNumbers = new Dictionary<FamilyInstance, string>();
            // Create dictionary to store number of windows in room
            Dictionary<string, int> instancesInRoomCount = new Dictionary<string, int>();

            // Design option filter
            ElementDesignOptionFilter designOptionFilter = new ElementDesignOptionFilter(ElementId.InvalidElementId);

            // Select phase
            PhaseArray aPhase = doc.Phases;
            Phase phase = aPhase.get_Item(aPhase.Size - 1);

            ElementId idPhase = phase.Id;
            ParameterValueProvider provider = new ParameterValueProvider(new ElementId((int)BuiltInParameter.PHASE_CREATED));
            FilterNumericRuleEvaluator evaluator = new FilterNumericEquals();
            FilterElementIdRule rule = new FilterElementIdRule(provider, evaluator, idPhase);
            ElementParameterFilter paraFilter = new ElementParameterFilter(rule);

            // Collect all windows in project
            using (FilteredElementCollector instances = new FilteredElementCollector(doc).OfCategory(builtInCategory)
                                            .WhereElementIsNotElementType().WherePasses(designOptionFilter).WherePasses(paraFilter))
            {
                string roomNumber = "";
                // Retrieve rooms from windows
                foreach (FamilyInstance inst in instances)
                {
                    if (builtInCategory == BuiltInCategory.OST_Doors)
                    {
                        if (inst.ToRoom != null)
                        {
                            roomNumber = inst.ToRoom.Number;
                        }
                        else if (inst.FromRoom != null)
                        {
                            roomNumber = inst.FromRoom.Number;
                        }
                    }
                    else
                    {
                        if (inst.FromRoom != null)
                        {
                            roomNumber = inst.FromRoom.Number;
                        }
                        else if (inst.ToRoom != null)
                        {
                            roomNumber = inst.ToRoom.Number;
                        }
                    }

                    Helpers.InstanceFromToRoom(instancesInRoomCount, roomNumber, instanceNumbers, inst);
                }
            }

            // Create transaction and make changes in Revit
            using (Transaction t = new Transaction(doc, "Number Instances"))
            {
                t.Start();

                // Empty Mark parameter to avoid duplicated values
                foreach (KeyValuePair<FamilyInstance, string> entry in instanceNumbers)
                {
                    Parameter instanceMark = entry.Key.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                    instanceMark.Set("");
                }

                // Populate Mark parameter
                foreach (KeyValuePair<FamilyInstance, string> entry in instanceNumbers)
                {
                    Parameter instanceMark = entry.Key.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                    instanceMark.Set(entry.Value);
                }

                t.Commit();
            }
        }

        /// <summary>
        /// A function to count number of windows or doors in room
        /// </summary>
        /// <param name="instanceInRoomCount"></param>
        /// <param name="roomNumber"></param>
        /// <param name="instanceNumbers"></param>
        /// <param name="familyInstance"></param>
        public static void InstanceFromToRoom(Dictionary<string, int> instanceInRoomCount,
                                              string roomNumber,
                                              Dictionary<FamilyInstance, string> instanceNumbers,
                                              FamilyInstance familyInstance)
        {
            // Classify family instances
            if (instanceInRoomCount.ContainsKey(roomNumber))
            {
                // Check instance count is one, a prefix will be added to the number
                if (instanceInRoomCount[roomNumber] == 1)
                {
                    // Retrieve instance with single count from the dictionary that stores instance-room relationship
                    FamilyInstance keyInstance = instanceNumbers.FirstOrDefault(x => x.Value == roomNumber).Key;
                    // Change single count instance prefix
                    instanceNumbers[keyInstance] = roomNumber + "-" + numberToLetter(instanceInRoomCount[roomNumber] - 1);
                    // Store current instance and room in dict
                    instanceNumbers[familyInstance] = roomNumber + "-" + numberToLetter(instanceInRoomCount[roomNumber]);
                }
                else
                {
                    // Store current instance and room in dict
                    instanceNumbers[familyInstance] = roomNumber + "-" + numberToLetter(instanceInRoomCount[roomNumber]);
                }
                // Increase the instance count in room
                instanceInRoomCount[roomNumber] = instanceInRoomCount[roomNumber] + 1;
            }
            else
            {
                // No instance is in room , add instance-room to dict
                instanceNumbers.Add(familyInstance, roomNumber);
                instanceInRoomCount.Add(roomNumber, 1);
            }
        }

        /// <summary>
        /// Map a number to a letter
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        static string numberToLetter(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
            {
                int orderPrefix = index / letters.Length;
                index = index % letters.Length;

                value += orderPrefix.ToString();
            }

            value += letters[index];

            return value;
        }

        /// <summary>
        /// A function to check if view is on any sheet
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static bool IsViewOnSheet(Document doc, View view)
        {
            // Element Id of view to compare
            ElementId viewId = view.Id;

            // Select all sheets
            List<ViewSheet> sheets = new FilteredElementCollector(doc)
                                    .OfClass(typeof(ViewSheet))
                                    .WhereElementIsNotElementType()
                                    .Cast<ViewSheet>()
                                    .Where(i => i.IsPlaceholder == false)
                                    .ToList();

            // Check if views placed on sheet match view
            foreach (ViewSheet sheet in sheets)
            {
                // Collect all views on sheets
                ISet<ElementId> viewsOnSheets = sheet.GetAllPlacedViews();

                // Compare views on sheet to view to check
                foreach (ElementId eId in viewsOnSheets)
                {
                    if (eId == viewId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
