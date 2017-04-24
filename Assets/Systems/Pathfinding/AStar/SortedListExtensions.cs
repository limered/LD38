using System.Collections.Generic;
/// <summary>
/// Extension methods to make the System.Collections.Generic.SortedList easier to use.
/// </summary>
public static class SortedListExtensions
{
    /// <summary>
    /// Adds a INode to the SortedList.
    /// </summary>
    /// <param name="sortedList">SortedList to add the node to.</param>
    /// <param name="node">Node to add to the sortedList.</param>
    public static void Add(this SortedList<int, INode> sortedList, INode node)
    {
        sortedList.Add(node.TotalCost, node);
    }

    /// <summary>
    /// Removes the node from the sorted list with the smallest TotalCost and returns that node.
    /// </summary>
    /// <param name="sortedList">SortedList to remove and return the smallest TotalCost node.</param>
    /// <returns>Node with the smallest TotalCost.</returns>
    public static INode Pop(this SortedList<int, INode> sortedList)
    {
        var top = sortedList.Values[0];
        sortedList.RemoveAt(0);
        return top;
    }
}