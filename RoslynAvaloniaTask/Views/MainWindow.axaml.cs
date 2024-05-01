using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace RoslynAvaloniaTask.Views;

public partial class MainWindow : Window
{
    private readonly TextEditor _textEditor;
    //private RegistryOptions _registryOptions;
    private TextMateSharp.Grammars.RegistryOptions _registryOptions;
    public MainWindow()
    {
        InitializeComponent();
        _textEditor = this.FindControl<TextEditor>("Editor");
        _textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
        _textEditor.Background = Brushes.Transparent;
        _textEditor.ShowLineNumbers = true;
        
        _registryOptions = new TextMateSharp.Grammars.RegistryOptions(ThemeName.DarkPlus);
        
        var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Update current line highlight when caret index changes
    }

}