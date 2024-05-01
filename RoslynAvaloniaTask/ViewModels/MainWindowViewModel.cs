using System;
using System.Collections.Generic;
using System.IO;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
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
    private string DisplayText
    {
        get => _displayText;
        set => this.RaiseAndSetIfChanged(ref _displayText, value);
    }
    private string _syntaxTreeText;
    public string SyntaxTreeText
    {
        get => _syntaxTreeText;
        set => this.RaiseAndSetIfChanged(ref _syntaxTreeText, value);
    }
    private int _windowHeight;
    public int WindowHeight
    {
        get => _windowHeight;
        set => this.RaiseAndSetIfChanged(ref _windowHeight, value);
    }
    public ICommand VisualizeCodeCommand { get; }
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public MainWindowViewModel()
    {
        VisualizeCodeCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    Traverse();
                }
                catch (Exception e)
                {
                    SyntaxTreeText = $"Error: {e.Message}";
                }
            }
        );
    }

    private void Traverse()
    {
        int level = 0;
        /* initializing stack for DFS on a syntax tree */
        Stack<(SyntaxNodeOrToken tokenOrNode, int level)> myStack =
            new Stack<(SyntaxNodeOrToken tokenOrNode, int level)>();
        HashSet<SyntaxNodeOrToken> visitedSet = new HashSet<SyntaxNodeOrToken>();
        //Console.WriteLine(CodeText);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(CodeText);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
        
        myStack.Push((root, 0));
        visitedSet.Add(root);
        
        SyntaxTreeText = "";

        while (myStack.Count != 0)
        {
            var current = myStack.Pop();
            string currString = "";
            for (int i = 0; i < current.level; i++)
                currString += "|\t";
            
            // Check if it's a node or token
            if (current.tokenOrNode.IsNode)
            {
                // If it's a node, you can cast it to SyntaxNode and further investigate
                SyntaxNode node = current.tokenOrNode.AsNode();
                currString += $"Node: [{node.Kind()}]\n";
                SyntaxTreeText += currString + "\n";
            }
            else
            {
                 //If it's a token, you can cast it to SyntaxToken and further investigate
                 SyntaxToken token = current.tokenOrNode.AsToken();
                 currString += $"Token: [{token.Kind()}]\n";
                SyntaxTreeText += currString + "\n";
            }
            
            foreach (var tokenOrNode in current.tokenOrNode.ChildNodesAndTokens())
            {
                if (visitedSet.Add(tokenOrNode))
                    myStack.Push((tokenOrNode, current.level + 1));
            }
        }
        
        string rootString = root.ToString();
        var rootKind = root.Kind();
        var rootKindString = rootKind.ToString();
    }
}