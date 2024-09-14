using University.Controls;
using University.Interfaces;

namespace University.Services;

public class DialogService : IDialogService
{
    public bool? Show(string itemName)
    {
        ConfirmationDialog confirmationDialog = new ConfirmationDialog(itemName);
        return confirmationDialog.ShowDialog();
    }
}
