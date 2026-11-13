using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zentro.Models;

namespace Zentro.Views;

public partial class BoardColumn : UserControl
{
    public static readonly DependencyProperty ColumnTitleProperty = DependencyProperty.Register(nameof(ColumnTitle), typeof(string), typeof(BoardColumn));
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items), typeof(WorkspaceItem[]), typeof(BoardColumn));
    public static readonly DependencyProperty ToneProperty = DependencyProperty.Register(nameof(Tone), typeof(Brush), typeof(BoardColumn));
    public static readonly DependencyProperty CardTemplateProperty = DependencyProperty.Register(nameof(CardTemplate), typeof(DataTemplate), typeof(BoardColumn));
    public string ColumnTitle { get => (string)GetValue(ColumnTitleProperty); set => SetValue(ColumnTitleProperty, value); }
    public WorkspaceItem[] Items { get => (WorkspaceItem[])GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public Brush Tone { get => (Brush)GetValue(ToneProperty); set => SetValue(ToneProperty, value); }
    public DataTemplate CardTemplate { get => (DataTemplate)GetValue(CardTemplateProperty); set => SetValue(CardTemplateProperty, value); }
    public event EventHandler? AddRequested;

    public BoardColumn() => InitializeComponent();
    private void AddClick(object sender, RoutedEventArgs e) => AddRequested?.Invoke(this, EventArgs.Empty);
}
