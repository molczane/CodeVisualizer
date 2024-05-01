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
                    //Console.WriteLine(CodeText);
                    // SyntaxTree tree = CSharpSyntaxTree.ParseText(CodeText);
                    // CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                    //
                    // StringWriter stringWriter = new StringWriter();
                    // tree.GetRoot().WriteTo(stringWriter);
                    //
                    // SyntaxTreeText = "";
                    // // SyntaxTreeText += stringWriter.ToString();
                    //
                    // string rootString = root.ToString();
                    // var rootKind = root.Kind();
                    // var rootKindString = rootKind.ToString();
                    //
                    // var children = root.ChildNodesAndTokens();
                    //
                    // foreach (var child in children)
                    // {
                    //     // Check if it's a node or token
                    //     if (child.IsNode)
                    //     {
                    //         // If it's a node, you can cast it to SyntaxNode and further investigate
                    //         SyntaxNode node = child.AsNode();
                    //         SyntaxTreeText += $"Node: {node.Kind()}\n";
                    //         Console.WriteLine($"Node: {node.Kind()}"); // Print the kind of the node
                    //
                    //         // If you want to recursively explore the children of this node, you can do so
                    //         // For example:
                    //         foreach (var grandChild in node.ChildNodesAndTokens())
                    //         {
                    //             if (grandChild.IsNode)
                    //             {
                    //                 SyntaxNode grandChildNode = grandChild.AsNode();
                    //                 SyntaxTreeText += $"  - Node: {grandChildNode.Kind()}\n";
                    //                 Console.WriteLine(
                    //                     $"  - Node: {grandChildNode.Kind()}"); // Print the kind of the grandchild node
                    //             }
                    //             else
                    //             {
                    //                 SyntaxToken grandChildToken = grandChild.AsToken();
                    //                 SyntaxTreeText += $"  - Token: {grandChildToken.Kind()}\n";
                    //                 Console.WriteLine(
                    //                     $"  - Token: {grandChildToken.Kind()}"); // Print the kind of the grandchild token
                    //             }
                    //         }
                    //     }
                    //     else
                    //     {
                    //         // If it's a token, you can cast it to SyntaxToken and further investigate
                    //         SyntaxToken token = child.AsToken();
                    //         SyntaxTreeText += $"Token: {token.Kind()}\n";
                    //         Console.WriteLine($"Token: {token.Kind()}"); // Print the kind of the token
                    //     }
                    // }
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
                currString += "\t";
            SyntaxTreeText += (currString + "\n");
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