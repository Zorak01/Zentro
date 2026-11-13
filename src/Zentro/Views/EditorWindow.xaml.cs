using System.Windows;
using System.Windows.Input;
using Zentro.Models;
using Zentro.ViewModels;

namespace Zentro.Views;

public partial class EditorWindow : Window
{
    private readonly WorkspaceItem original;
    private readonly MainViewModel workspace;

    public EditorWindow(WorkspaceItem item, bool existing, MainViewModel workspace)
    {
        InitializeComponent();
        original = item;
        this.workspace = workspace;
        Title = Heading.Text = $"{(existing ? "Edit" : "New")} {(item.Note ? "note" : "task")}";
        SaveButton.Content = item.Note ? "Save note" : "Save task";
        BodyLabel.Content = item.Note ? "_Text" : "_Details";
        TitleBox.Text = item.Title;
        BodyBox.Text = item.Body;
        StatusBox.ItemsSource = WorkspaceOptions.Statuses;
        StatusBox.SelectedItem = item.Status;
        PriorityBox.ItemsSource = WorkspaceOptions.Priorities;
        PriorityBox.SelectedItem = item.Priority;
        TaskOptions.Visibility = item.Note ? Visibility.Collapsed : Visibility.Visible;
        DeleteButton.Visibility = existing ? Visibility.Visible : Visibility.Collapsed;
        Loaded += (_, _) => TitleBox.Focus();
        PreviewKeyDown += (_, e) => { if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) { Save(); e.Handled = true; } };
    }

    private void SaveClick(object sender, RoutedEventArgs e) => Save();

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            ErrorText.Text = "Enter a title.";
            TitleBox.Focus();
            return;
        }
        var item = original with
        {
            Title = TitleBox.Text.Trim(),
            Body = BodyBox.Text,
            Status = (string)StatusBox.SelectedItem,
            Priority = (string)PriorityBox.SelectedItem,
            Updated = DateTime.Now
        };
        string? error = workspace.Commit(item);
        if (error != null)
        {
            ErrorText.Text = error;
            return;
        }
        DialogResult = true;
    }

    private void DeleteClick(object sender, RoutedEventArgs e)
    {
        string? error = workspace.Delete(original);
        if (error != null)
        {
            ErrorText.Text = error;
            return;
        }
        DialogResult = true;
    }
}
