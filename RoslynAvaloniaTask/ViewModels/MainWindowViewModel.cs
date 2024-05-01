using System;
using System.Runtime.InteropServices.ComTypes;
using ReactiveUI;
using System.Windows.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynAvaloniaTask.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _codeText;
    public string CodeText
    {
        get => _codeText;
        set => this.RaiseAndSetIfChanged(ref _codeText, value);
    }
    private string _displayText;
    public string DisplayText
    {
        get => _displayText;
        set => this.RaiseAndSetIfChanged(ref _displayText, value);
    }
    public ICommand VisualizeCodeCommand { get; }
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public MainWindowViewModel()
    {
        VisualizeCodeCommand = ReactiveCommand.Create(() =>
            {
                Console.WriteLine(CodeText);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(CodeText);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                DisplayText = "";
                DisplayText += $"The tree is a {root.Kind()} node.\n";
                DisplayText += $"The tree has {root.Members.Count} elements in it.\n";
                DisplayText += $"The tree has {root.Usings.Count} using statements. They are:\n";
                foreach (UsingDirectiveSyntax element in root.Usings)
                    DisplayText += $"\t{element.Name}\n";
            }
            );
    }
}