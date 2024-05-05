Project done for participation in applicating for internship at JetBrains at project "Roslyn Syntax Visualizer for Rider IDE".

**SCOPE OF A PROJECT:**

You are required to develop a UI application (WPF, Windows Forms, Avalinoa, …) that demonstrates the visualization of syntax trees using Roslyn API.

Functional Requirements:
- Develop a basic graphical user interface, provide a text box to input C# code.
- Use Roslyn API to generate the syntax tree for the provided code.
- Display the generated syntax tree in a visual format within the GUI.
- Implement a feature to highlight (and/or navigate) selected syntax tree nodes in the code editor.

This task will allow you to demonstrate your C# development skills, interaction with Roslyn API, and ability to create intuitive and functional user interfaces.

**IMPLEMENTATION**

To implement my project I chose Avalonia UI Framework. I was developing this project on MacOS Operating System so this cross-platform UI framework was a perfect match for me.

Project was done for .NET 8.0 as a target machine.

Before I explain the details here are required NuGet packages to run this program on your computer:
- Avalonia.AvaloniaEdit
- Avalonia.Desktop
- Avalonia.Diagnostics
- Avalonia.Fonts.Inter
- Avalonia.ReactiveUI
- Avalonia.Themes.Fluent
- Avalonia
- AvaloniaEdit.TextMate
- TextMateSharp.Grammars

I started with creating basic window in Avalonia UI. Here is what it looks like:

![UILook](https://github.com/molczane/CodeVisualizer/assets/128298715/14d48e8a-c6e3-4b72-8a8a-c3b5c1d808a4)

Then I focused on creating Code editor for a user with syntax highlighting. For this particular functionality I chose the AvaloniaEdit control: TextEditor. Then I applied C# syntax checking and highlighting in it and this is how it looks like:

![Zrzut ekranu 2024-05-5 o 23 06 53](https://github.com/molczane/CodeVisualizer/assets/128298715/6683afae-52f2-4e6e-8a96-e2dc47067640)

It also checks for basic syntax errors (missing variable identifier here):

![Zrzut ekranu 2024-05-5 o 23 08 44](https://github.com/molczane/CodeVisualizer/assets/128298715/f382702a-7f74-4a4b-879a-22bda8497b2d)

Next part was getting familiar with Roslyn API and generating syntax tree. For this functionality I chose printing Roslyn Syntax tree of a code in a basic TextBox in form of a tree. To create a tree I am traversing a tree with a DFS algorithm and adding strings to TextBox text. To generate a syntax tree you simply click "Visualize Code" button.  This is how to tree looks like after generating it from source code:

![Zrzut ekranu 2024-05-5 o 23 13 13](https://github.com/molczane/CodeVisualizer/assets/128298715/675c2bfe-f505-41f8-a52b-fd5879b8fa1c)

I know it doesn't look great, but for now, it is what it is.

The last part was navigating and exploring source code based on Node or Token in a tree. Each line is connected to a SyntaxNodeOrToken object in a dictionary, and based on that I can say about which Node or Token is the current line. You navigate the tree by simply double cllicking the Node or Token line, and then in the Code Editor you can see which piece of code this Node or Token text is about. Here is how it works:

Example 1 - double clicked NamespaceDeclaration node:

![Zrzut ekranu 2024-05-5 o 23 24 33](https://github.com/molczane/CodeVisualizer/assets/128298715/14b9bcff-853e-4352-afe2-2e4e2890e6d3)

Example 2 - double clicked MethodDeclaration node:

![Zrzut ekranu 2024-05-5 o 23 25 28](https://github.com/molczane/CodeVisualizer/assets/128298715/61461834-6a48-42f9-936b-0ce0263e0861)

Example 3 - double clicked Argument node:

![Zrzut ekranu 2024-05-5 o 23 26 21](https://github.com/molczane/CodeVisualizer/assets/128298715/ce090cae-6f6a-4d11-acb4-d616725a38af)

I also tried implementing code completion, but I didn't make it work.

All the process of creating this program should be readable through commits.

The whole program logic lies in MainWindow.axaml.cs file.

I am aware of the fact that a lot of things could be done better in this project, but I am happy that I implemented features that I described. 