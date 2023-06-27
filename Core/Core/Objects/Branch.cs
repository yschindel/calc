﻿using System;
using System.Collections.Generic;
using System.Linq;
using Calc.Core.Color;
using Speckle.Newtonsoft.Json;

namespace Calc.Core.Objects
{
    public class Branch : IFilter
    {
        [JsonIgnore]
        public List<CalcElement> Elements { get; set; }
        [JsonIgnore]
        public List<string> ElementIds => Elements.Select(e => e.Id).ToList();
        [JsonIgnore]
        public string Parameter { get; set; }
        [JsonIgnore]
        public string Method { get; set; }
        [JsonIgnore]
        public string Value { get; set; }
        [JsonIgnore]
        public int BranchLevel = 0;
        [JsonIgnore]
        public List<Branch> SubBranches { get; set; } = new List<Branch>();
        [JsonIgnore]
        private Buildup _buildup;
        [JsonIgnore]
        public Buildup Buildup
        {
            get => _buildup;
            set => SetBuildup(value);
        }
        [JsonIgnore]
        public HslColor HslColor { get; set; } // Use conversion methods from Calc.Core.Colors on this as needed

        public Branch()
        {
            Parameter = "No Parameter";
            Method = "No Method";
            Value = "No Value";
            HslColor = new HslColor(42, 42, 42);
        }

        public Branch(List<CalcElement> elements) : this()
        {
            Elements = elements;
        }

        public void CreateBranches(List<string> branchConfig)
        {
            if (branchConfig.Count <= BranchLevel)
            {
                return;
            }

            var currentParameter = branchConfig[BranchLevel];
            var groupedElements = GroupByParameterValue(currentParameter);
            var methodName = nameof(GroupByParameterValue);

            foreach (KeyValuePair<object, List<CalcElement>> group in groupedElements)
            {
                var branch = new Branch(group.Value)
                {
                    Parameter = currentParameter,
                    Value = group.Key.ToString(),
                    Method = methodName
                };
                SubBranches.Add(branch);
            }

            if (SubBranches.Count == 0)
            {
                return;
            }

            for (int index = 0; index < SubBranches.Count; index++)
            {
                var subBranch = SubBranches[index];
                subBranch.BranchLevel = BranchLevel + 1;
                subBranch.CreateBranches(branchConfig);
            }
        }

        public void SetBuildup(Buildup buildup)
        {
            // set the buildup of the current branch. Also set the buildup of all subbranches to the same value if they have no buildup assigned yet or the buildup is the same.
            var currentBuildup = _buildup;
            _buildup = buildup;
            if (SubBranches.Count == 0)
            {
                return;
            }

            foreach (var subBranch in SubBranches)
            {
                if (subBranch.Buildup == null || subBranch.Buildup == currentBuildup)
                {
                    subBranch.SetBuildup(buildup);
                }
            }
        }

        public void ClearMapping()
        {
            _buildup = null;
            if (SubBranches.Count == 0)
            {
                return;
            }

            foreach (var subBranch in SubBranches)
            {
                subBranch.ClearMapping();
            }
        }

        /// <summary>
        /// This method matches a buildup to a branch.
        /// The intended use is to reconstruct the buildup assignments in a full tree from 
        /// a mapping stored in the database.
        /// </summary>
        public void MatchMapping(string parameter, string value, Buildup buildup)
        {
            if (Parameter == parameter && Value == value)
            {
                this._buildup = buildup;
            }
            else
            {
                // check if subbranches exist
                if (SubBranches.Count == 0)
                {
                    return;
                }
                foreach (var subBranch in SubBranches)
                {
                    subBranch.MatchMapping(parameter, value, buildup);
                }
            }
        }

        /// <summary>
        /// WARNING: DESTRUCTIVE METHOD - USE ONLY ON A COPY OF THE TREE
        /// The Intended use is right befor calculation.
        /// 
        /// This method removes elements from the current branch that
        /// are also present in subbranches that have a buildup assigned.
        /// In the bigger picture, this allows to override buildups further down the tree.
        /// </summary>
        public void RemoveElementsByBuildupOverrides()
        {
            if (Buildup != null && SubBranches.Count > 0)
            {
                // get all elements with buildup from subbranches
                var subElementsWithBuildup = SubBranches
                    .Where(sb => sb.Buildup != null)
                    .SelectMany(sb => sb.Elements)
                    .ToList();
                // remove subelements with buildup from elements of current branch
                Elements = Elements
                    .Where(e => !subElementsWithBuildup.Contains(e))
                    .ToList();
            }
            if (SubBranches.Count > 0)
            {
                foreach (var subBranch in SubBranches)
                {
                    subBranch.RemoveElementsByBuildupOverrides();
                }
            }
        }

        /// <summary>
        /// This method returns a flat list of all subbranches. The current branch is included.
        /// Preferrably use this method on the root branch instance aka tree.Trunk.
        /// The output is useful when you want to iterate over all branches
        /// but don't care about the tree structure anymore.
        /// </summary>
        public List<Branch> Flatten()
        {
            var branches = new List<Branch> { this };
            if (SubBranches.Count > 0)
            {
                foreach (var subBranch in SubBranches)
                {
                    branches.AddRange(subBranch.Flatten());
                }
            }
            return branches;
        }

        public List<int> GetAllBranchLevels()
        {
            List<int> levels = new() { BranchLevel };

            foreach (var subBranch in SubBranches)
            {
                levels.AddRange(subBranch.GetAllBranchLevels());
            }

            levels.Sort();
            return levels.Distinct().ToList();
        }

        /// <summary>
        /// Returns a list of element ids at a given level.
        /// Call this method on the root branch instance.
        /// </summary>
        /// <param name="level">The level to get element ids from.</param>
        /// <returns>A list of element ids.</returns>
        public List<string> GetElementIdsAtLevel(int level)
        {
            List<string> ids = new();

            if (BranchLevel == level)
            {
                ids.AddRange(ElementIds);
            }
            else
            {
                foreach (var subBranch in SubBranches)
                {
                    ids.AddRange(subBranch.GetElementIdsAtLevel(level));
                }
            }

            return ids;
        }

        public void PrintTree(int indentLevel = 0)
        {
            string indentation = new(' ', indentLevel * 4);
            Console.WriteLine($"{indentation}L: {BranchLevel}, Param: {Parameter}, Val: {Value}, Meth: {Method}, Col: {HslColor.Hue}, Build: {Buildup}");

            foreach (var subBranch in SubBranches)
            {
                subBranch.PrintTree(indentLevel + 1);
            }
        }

        private Dictionary<object, List<CalcElement>> GroupByParameterValue(string parameter)
        {
            var groupedElements = new Dictionary<object, List<CalcElement>>();
            var nullKey = new object(); // Sentinel value for null

            foreach (var element in this.Elements)
            {
                if (element.Fields.TryGetValue(parameter, out object value))
                {
                    var key = value ?? nullKey;

                    if (groupedElements.ContainsKey(key))
                    {
                        groupedElements[key].Add(element);
                    }
                    else
                    {
                        groupedElements[key] = new List<CalcElement> { element };
                    }
                }
            }
            return groupedElements;
        }
    }
}
