// File: ui/DotNetDllInvoker.UI/ViewModels/CallGraphViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for the Call Graph visualization window.
// Manages nodes, edges, selection, and layout.
//
// Depends on:
// - DotNetDllInvoker.Reflection (CallGraphAnalyzer)
//
// Used by:
// - CallGraphWindow.xaml (DataContext)
// - MainViewModel (creates and shows)
//
// Execution Risk:
// None. UI logic only.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DotNetDllInvoker.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class CallGraphViewModel : ViewModelBase
{
    private CallGraphNodeViewModel? _selectedNode;
    private string _statusText = "Ready";

    public CallGraphViewModel()
    {
        Nodes = new ObservableCollection<CallGraphNodeViewModel>();
        Edges = new ObservableCollection<CallGraphEdgeViewModel>();
        RefreshCommand = new RelayCommand(ExecuteRefresh);
    }

    public ObservableCollection<CallGraphNodeViewModel> Nodes { get; }
    public ObservableCollection<CallGraphEdgeViewModel> Edges { get; }

    public CallGraphNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (SetProperty(ref _selectedNode, value))
            {
                OnPropertyChanged(nameof(SelectedNodeIL));
                OnPropertyChanged(nameof(SelectedNodeCode));
                
                // Highlight edges connected to selected node
                HighlightSelectedEdges(value);
            }
        }
    }
    
    private void HighlightSelectedEdges(CallGraphNodeViewModel? node)
    {
        int selectedCount = 0;
        foreach (var edge in Edges)
        {
            bool isConnected = node != null && 
                (edge.From.Id == node.Id || edge.To.Id == node.Id);
            edge.IsHighlighted = isConnected;
            if (isConnected) selectedCount++;
        }
        
        // Update status with both counts
        var nodeInfo = node != null ? $" | Selected: {node.DisplayName} ({selectedCount} connections)" : "";
        StatusText = $"Nodes: {Nodes.Count} | Total Edges: {Edges.Count}{nodeInfo}";
    }

    public string SelectedNodeIL => _selectedNode?.GetILCode() ?? "// Select a node to view IL";
    public string SelectedNodeCode => _selectedNode?.GetCSharpCode() ?? "// Select a node to view code";

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Loads a call graph from an assembly.
    /// </summary>
    public void LoadFromAssembly(Assembly assembly)
    {
        StatusText = $"Analyzing {assembly.GetName().Name}...";
        
        try
        {
            var analyzer = new CallGraphAnalyzer();
            var graph = analyzer.BuildGraph(assembly);
            
            PopulateFromGraph(graph);
            
            StatusText = $"Loaded {Nodes.Count} nodes, {Edges.Count} edges";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Loads a call graph starting from a specific method.
    /// </summary>
    public void LoadFromMethod(MethodBase method, int depth = 3)
    {
        StatusText = $"Analyzing {method.Name}...";
        
        try
        {
            var analyzer = new CallGraphAnalyzer();
            var graph = analyzer.BuildGraph(method, depth);
            
            PopulateFromGraph(graph);
            
            StatusText = $"Loaded {Nodes.Count} nodes, {Edges.Count} edges (depth {depth})";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    private void PopulateFromGraph(CallGraph graph)
    {
        Nodes.Clear();
        Edges.Clear();
        
        // Create node ViewModels first (without positions)
        foreach (var node in graph.Nodes.Values)
        {
            var vm = new CallGraphNodeViewModel(node) { X = 0, Y = 0 };
            Nodes.Add(vm);
        }
        
        // Create edge ViewModels (need this to determine hierarchy)
        foreach (var edge in graph.Edges)
        {
            var fromNode = Nodes.FirstOrDefault(n => n.Id == edge.FromId);
            var toNode = Nodes.FirstOrDefault(n => n.Id == edge.ToId);
            
            if (fromNode != null && toNode != null)
            {
                Edges.Add(new CallGraphEdgeViewModel(fromNode, toNode, edge.CallType));
            }
        }
        
        // Apply hierarchical layout
        ApplyHierarchicalLayout();
    }
    
    /// <summary>
    /// Hierarchical layout: Entry points at top, called methods below.
    /// Uses a simplified Sugiyama-style algorithm.
    /// </summary>
    private void ApplyHierarchicalLayout()
    {
        if (Nodes.Count == 0) return;
        
        // Step 1: Calculate node levels (depth from entry points)
        var nodeIds = new HashSet<string>(Nodes.Select(n => n.Id));
        var incomingCount = new Dictionary<string, int>();
        var level = new Dictionary<string, int>();
        
        foreach (var node in Nodes)
        {
            incomingCount[node.Id] = 0;
            level[node.Id] = 0;
        }
        
        foreach (var edge in Edges)
        {
            incomingCount[edge.To.Id]++;
        }
        
        // Entry points = nodes with no incoming edges
        var entryPoints = Nodes.Where(n => incomingCount[n.Id] == 0).ToList();
        if (entryPoints.Count == 0)
        {
            // No clear entry points (circular), just take first few
            entryPoints = Nodes.Take(3).ToList();
        }
        
        // BFS to assign levels
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        
        foreach (var entry in entryPoints)
        {
            level[entry.Id] = 0;
            queue.Enqueue(entry.Id);
            visited.Add(entry.Id);
        }
        
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var currentLevel = level[currentId];
            
            // Find all outgoing edges
            var outgoing = Edges.Where(e => e.From.Id == currentId).ToList();
            foreach (var edge in outgoing)
            {
                var targetId = edge.To.Id;
                var newLevel = currentLevel + 1;
                
                // Only update if deeper (handles cycles)
                if (newLevel > level[targetId])
                {
                    level[targetId] = newLevel;
                }
                
                if (!visited.Contains(targetId))
                {
                    visited.Add(targetId);
                    queue.Enqueue(targetId);
                }
            }
        }
        
        // Step 2: Group nodes by level
        var nodesPerLevel = Nodes.GroupBy(n => level[n.Id])
                                  .OrderBy(g => g.Key)
                                  .ToList();
        
        // Step 3: Position nodes
        const double levelHeight = 120;    // Vertical spacing between levels
        const double nodeWidth = 180;      // Horizontal spacing between nodes
        const double startX = 50;
        const double startY = 50;
        
        foreach (var levelGroup in nodesPerLevel)
        {
            var levelY = startY + levelGroup.Key * levelHeight;
            var nodesInLevel = levelGroup.ToList();
            var totalWidth = nodesInLevel.Count * nodeWidth;
            var startXForLevel = startX + (nodesInLevel.Count > 1 ? 0 : 200); // Center single nodes
            
            for (int i = 0; i < nodesInLevel.Count; i++)
            {
                nodesInLevel[i].X = startXForLevel + i * nodeWidth;
                nodesInLevel[i].Y = levelY;
            }
        }
    }

    private void ExecuteRefresh(object? obj)
    {
        // Re-apply hierarchical layout
        ApplyHierarchicalLayout();
    }
}

public class CallGraphNodeViewModel : ViewModelBase
{
    private readonly CallGraphNode _node;
    private double _x;
    private double _y;
    private bool _isSelected;
    private bool _hasOutgoingEdges;

    public CallGraphNodeViewModel(CallGraphNode node)
    {
        _node = node;
    }

    public string Id => _node.Id;
    public string DisplayName => _node.DisplayName;
    public bool IsExternal => _node.IsExternal;
    
    public System.Reflection.MethodBase? GetMethod() => _node.Method;

    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool HasOutgoingEdges
    {
        get => _hasOutgoingEdges;
        set => SetProperty(ref _hasOutgoingEdges, value);
    }

    public string GetILCode()
    {
        if (_node.Method == null) return "// No method info";
        
        try
        {
            var instructions = ILReader.Read(_node.Method);
            if (instructions.Count == 0) return "// No IL body";
            return string.Join("\n", instructions.Select(i => i.ToString()));
        }
        catch (Exception ex)
        {
            return $"// Error: {ex.Message}";
        }
    }

    public string GetCSharpCode()
    {
        if (_node.Method == null) return "// No method info";
        return DecompilerService.Decompile(_node.Method);
    }
}

public class CallGraphEdgeViewModel : ViewModelBase
{
    private bool _isHighlighted;
    
    public CallGraphEdgeViewModel(CallGraphNodeViewModel from, CallGraphNodeViewModel to, string callType)
    {
        From = from;
        To = to;
        CallType = callType;
        
        // Mark from node as having outgoing edges
        from.HasOutgoingEdges = true;
    }

    public CallGraphNodeViewModel From { get; }
    public CallGraphNodeViewModel To { get; }
    public string CallType { get; }
    
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetProperty(ref _isHighlighted, value);
    }
    
    // For Path binding with Points
    public System.Windows.Point StartPoint => new(From.X + 80, From.Y + 25);
    public System.Windows.Point EndPoint => new(To.X + 80, To.Y + 25);
    
    // Legacy line properties (kept for compatibility)
    public double X1 => From.X + 80;
    public double Y1 => From.Y + 25;
    public double X2 => To.X + 80;
    public double Y2 => To.Y + 25;
}

