﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BIMiconToolbar.Helpers
{
    class HelpersDirectory
    {
        /// <summary>
        /// Function to move directories
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void MoveDirectory(string[] source, string[] destination)
        {
            // Check source list is not empty
            if (Helpers.IsNullOrEmpty(source) || Helpers.IsNullOrEmpty(destination))
            {
                // TODO: Log error
            }
            // Move all directories in list
            else
            {
                for(int i = 0; i < source.Length; i++)
                {
                    Directory.Move(source[i], destination[i]);
                }
            }
        }

        /// <summary>
        /// Function to rename directories
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void RenameDirectory(string[] source, string oldChar, string newChar)
        {
            if (Helpers.IsNullOrEmpty(source) != true)
            {
                var destination = HelpersString.ReplaceString(source, oldChar, newChar);

                HelpersDirectory.MoveDirectory(source, destination);
            }
        }

        /// <summary>
        /// Retrieve all files in folder
        /// </summary>
        /// <param name="selectedDirectory"></param>
        public static string[] RetrieveFiles(string selectedDirectory)
        {
            try
            {
                var files = Directory.GetFiles(selectedDirectory);
                return files;
            }

            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                // TODO: log error

            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                // TODO: log error
            }

            return new string[0];
        }

        /// <summary>
        /// Retrieve subdirectories in directory
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static string[] GetDirectoriesFromPath(string directoryPath)
        {
            try
            {
                var subdirectories = Directory.GetDirectories(directoryPath);
                return subdirectories;
            }

            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                // TODO: log error
            }

            return new string[0];  
        }

        /// <summary>
        /// Function to match and extract extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFilePathExtension(string filePath)
        {
            var extension = "";

            Regex backupPattern = new Regex(@"\.\w{2,4}$");
            Match fileMatch = backupPattern.Match(filePath);

            if (fileMatch.Success)
            {
                extension = fileMatch.Value;
            }

            return extension;
        }

        /// <summary>
        /// Function to retrieve file extension of files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static string[] GetFilesType(string[] files)
        {
            var fileTypes = new List<string>();

            if (Helpers.IsNullOrEmpty(files) != true)
            {
                foreach (string f in files)
                {
                    string match = GetFilePathExtension(f);
                    if (match != "")
                    {
                        fileTypes.Add(match);
                    }
                }
            }

            return fileTypes.Distinct().ToArray();
        }

        /// <summary>
        /// Retrieve file name from path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileFromFilePath(string filePath)
        {
            var fileName = "";

            if (filePath != null && filePath != "")
            {
                Regex filePattern = new Regex(@"\\[\w\d-]+\.\w{2,4}$");
                Match fileMatch = filePattern.Match(filePath);

                if (fileMatch.Success)
                {
                    fileName = fileMatch.Value.Remove(0, 1);
                }
            }

            return fileName;
        }

        /// <summary>
        /// Function to return first file name from file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFirstFileFromFilePath(string filePath)
        {
            string fileName = "";
            
            if (filePath != null && filePath != "")
            {
                string filePaths = RetrieveFiles(filePath)[0];
                
                if (filePaths.Length != 0)
                {
                    string firstFilePath = RetrieveFiles(filePath)[0];
                    fileName = GetFileFromFilePath(firstFilePath);
                }
            }

            return fileName;
        }

        /// <summary>
        /// Function to update a path name
        /// </summary>
        /// <param name="folderOrFile"></param>
        /// <param name="selectedPath"></param>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string UpdatePathName(bool folderOrFile,
                                            string selectedPath,
                                            string find,
                                            string replace,
                                            string prefix,
                                            string suffix)
        {
            string originalName = "";

            if (folderOrFile)
            {
                string fileName = GetFirstFileFromFilePath(selectedPath);
                originalName = fileName;
            }
            else
            {
                string folderName = GetDirectoriesFromPath(selectedPath)[0];
                originalName = folderName;
            }

            if (find != null && replace != null)
            {
                originalName = originalName.Replace(find, replace);
            }

            string updatedName = selectedPath + "\\" + prefix + originalName + suffix;

            return updatedName;
        }
    }
}
