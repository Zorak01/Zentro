using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Zentro.Models;
using Zentro.ViewModels;

namespace Zentro.Views;

public partial class MainWindow : Window
{
    private const string DragFormat = "Zentro.Task";
    private readonly MainViewModel model = new();
    private Point dragStart;
    private bool dragged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = model;
        PreviewKeyDown += OnShortcut;
    }

    private void MinimizeClick(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
    private void MaximizeClick(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
        else SystemCommands.MaximizeWindow(this);
    }
    private void CloseClick(object sender, RoutedEventArgs e) => Close();
    private void NewColumn(object? sender, EventArgs e)
    {
        if (sender is BoardColumn column) OpenEditor(new WorkspaceItem { Status = column.ColumnTitle });
    }

    private void BoardClick(object sender, RoutedEventArgs e) => model.IsNotes = false;
    private void NotesClick(object sender, RoutedEventArgs e) => model.IsNotes = true;
    private void ClearSearch(object sender, RoutedEventArgs e) { model.Query = ""; SearchBox.Focus(); }
    private void UndoClick(object sender, RoutedEventArgs e) => model.Undo();
    private void NewClick(object sender, RoutedEventArgs e) => OpenEditor(new WorkspaceItem { Note = model.IsNotes, Status = (sender as Button)?.Tag as string ?? "To do" });

    private void OpenEditor(WorkspaceItem item, bool existing = false)
    {
        if (!model.CanWrite) return;
        new EditorWindow(item, existing, model) { Owner = this }.ShowDialog();
    }

    private void CardDown(object sender, MouseButtonEventArgs e)
    {
        dragStart = e.GetPosition(this);
        dragged = false;
        ((Border)sender).Focus();
    }

    private void CardMove(object sender, MouseEventArgs e)
    {
        var card = (Border)sender;
        if (e.LeftButton != MouseButtonState.Pressed || dragged || card.DataContext is not WorkspaceItem { Note: false } item || !model.CanWrite) return;
        var point = e.GetPosition(this);
        if (Math.Abs(point.X - dragStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(point.Y - dragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        dragged = true;
        DragDrop.DoDragDrop(card, new DataObject(DragFormat, item.Id), DragDropEffects.Move);
    }

    private void CardUp(object sender, MouseButtonEventArgs e)
    {
        if (!dragged && ((Border)sender).DataContext is WorkspaceItem item) OpenEditor(item, true);
    }

    private void CardKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && ((Border)sender).DataContext is WorkspaceItem item) { OpenEditor(item, true); e.Handled = true; }
    }

    private void ColumnOver(object sender, DragEventArgs e)
    {
        bool valid = model.CanWrite && e.Data.GetDataPresent(DragFormat);
        e.Effects = valid ? DragDropEffects.Move : DragDropEffects.None;
        if (valid) ((BoardColumn)sender).BorderBrush = (Brush)FindResource("Accent");
        e.Handled = true;
    }

    private void ColumnLeave(object sender, DragEventArgs e) => ((BoardColumn)sender).ClearValue(Control.BorderBrushProperty);

    private void ColumnDrop(object sender, DragEventArgs e)
    {
        var column = (BoardColumn)sender;
        column.ClearValue(Control.BorderBrushProperty);
        if (e.Data.GetData(DragFormat) is string id && column.ColumnTitle is string status) model.Move(id, status);
        e.Handled = true;
    }

    private void OnShortcut(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        if (e.Key == Key.F) { SearchBox.Focus(); SearchBox.SelectAll(); e.Handled = true; }
        if (e.Key == Key.N) { OpenEditor(new WorkspaceItem { Note = model.IsNotes }); e.Handled = true; }
        if (e.Key == Key.Z && Keyboard.FocusedElement is not TextBox) { model.Undo(); e.Handled = true; }
    }
}
