﻿using Calc.ConnectorRevit.ViewModels;

namespace Calc.ConnectorRevit.Helpers
{
    public class NodeHelper
    {
        public static void HideAllLabelColor(NodeViewModel forestItem)
        {    
            foreach (NodeViewModel nodeItem in forestItem.SubNodeItems)
            {
                HideNodeLabelColor(nodeItem);
            }
        }

        private static void HideNodeLabelColor(NodeViewModel nodeItem)
        {
            nodeItem.LabelColorVisible = false;

            foreach (NodeViewModel subNodeItem in nodeItem.SubNodeItems)
            {
                HideNodeLabelColor(subNodeItem);
            }
        }
        public static void ShowSubLabelColor(NodeViewModel nodeItem)
        {
            foreach (NodeViewModel subNodeItem in nodeItem.SubNodeItems)
            {
                subNodeItem.LabelColorVisible = true;
            }
        }

        public static void ShowAllSubLabelColor(NodeViewModel nodeItem)
        {
            nodeItem.LabelColorVisible = true;
            foreach (NodeViewModel subNodeItem in nodeItem.SubNodeItems)
            {
                ShowAllSubLabelColor(subNodeItem);
            }
        }
    }
}
