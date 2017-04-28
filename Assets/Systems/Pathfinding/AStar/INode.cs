//Source: https://raw.githubusercontent.com/jbaldwin/astar.cs/master/INode.cs

using System.Collections.Generic;

public interface INode
{
    /// <summary>
    /// Determines if this node is on the open list.
    /// </summary>
    bool IsOpenList(IEnumerable<INode> openList);

    /// <summary>
    /// Sets this node to be on the open list.
    /// </summary>
    void SetOpenList(bool value);

    /// <summary>
    /// Determines if this node is on the closed list.
    /// </summary>
    bool IsClosedList(IEnumerable<INode> closedList);

    /// <summary>
    /// Sets this node to be on the open list.
    /// </summary>
    void SetClosedList(bool value);

    /// <summary>
    /// Gets the total cost for this node.
    /// f = g + h
    /// TotalCost = MovementCost + EstimatedCost
    /// </summary>
    int TotalCost { get; }

    /// <summary>
    /// Gets the movement cost for this node.
    /// This is the movement cost from this node to the starting node, or g.
    /// </summary>
    int MovementCost { get; }

    /// <summary>
    /// Gets the estimated cost for this node.
    /// This is the heuristic from this node to the goal node, or h.
    /// </summary>
    int EstimatedCost { get; }

    /// <summary>
    /// Sets the movement cost for the current node, or g.
    /// </summary>
    /// <param name="parent">Parent node, for access to the parents movement cost.</param>
    void SetMovementCost(INode parent);

    /// <summary>
    /// Sets the estimated cost for the current node, or h.
    /// </summary>
    /// <param name="goal">Goal node, for acces to the goals position.</param>
    void SetEstimatedCost(INode goal);

    /// <summary>
    /// Gets or sets the parent node to this node.
    /// </summary>
    INode Parent { get; set; }

    /// <summary>
    /// Gets this node's children.
    /// </summary>
    /// <remarks>The children can be setup in a graph before starting the
    /// A* algorithm or they can be dynamically generated the first time
    /// the A* algorithm calls this property.</remarks>
    IEnumerable<INode> Children { get; }

    /// <summary>
    /// Returns true if this node is the goal, false if it is not the goal.
    /// </summary>
    /// <param name="goal">The goal node to compare this node against.</param>
    /// <returns>True if this node is the goal, false if it s not the goal.</returns>
    bool IsGoal(INode goal);
}