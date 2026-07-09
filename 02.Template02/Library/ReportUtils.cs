using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DVTools.Library
{
    public class ReportUtils
    {
        public static TaskDialogResult ShowDialogError(string nameDialog, string mainInstruction, string mainContent)
        {
            TaskDialog dialog = new TaskDialog(nameDialog);
            dialog.MainInstruction = mainInstruction;
            dialog.MainContent = mainContent;
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;
            dialog.MainIcon = TaskDialogIcon.TaskDialogIconError;

            TaskDialogResult result = dialog.Show();
            return result;
        }
        public static TaskDialogResult ShowDialogSuccess(string nameDialog, string mainInstruction, string mainContent)
        {
            TaskDialog dialog = new TaskDialog(nameDialog);
            dialog.MainInstruction = mainInstruction;
            dialog.MainContent = mainContent;
            dialog.CommonButtons = TaskDialogCommonButtons.Ok;
            dialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;

            TaskDialogResult result = dialog.Show();
            return result;
        }
        public static TaskDialogResult ShowDialogUnsuccessful(string nameDialog, string mainInstruction, string mainContent)
        {
            TaskDialog dialog = new TaskDialog(nameDialog);
            dialog.MainInstruction = mainInstruction;
            dialog.MainContent = mainContent;
            dialog.CommonButtons = TaskDialogCommonButtons.Ok;
            dialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;

            TaskDialogResult result = dialog.Show();
            return result;
        }
        public static MessageBoxResult ShowMessageSuccess(string title, string message, string detail)
        {
            string content = $"{message}\n\n{detail}";
            return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static MessageBoxResult ShowMessageUnsuccessful(string title, string message, string detail)
        {
            string content = $"{message}\n\n{detail}";
            return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static TaskDialogResult ShowDialogWaning(string nameDialog, string mainInstruction, string mainContent)
        {
            TaskDialog dialog = new TaskDialog(nameDialog);
            dialog.MainInstruction = mainInstruction;
            dialog.MainContent = mainContent;
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;
            dialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;

            TaskDialogResult result = dialog.Show();
            return result;
        }

    }
}
