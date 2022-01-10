using System;
using Avalonia.Controls;

namespace Avalonia.LogicalTree
{
    /// <summary>
    /// Represents a node in the logical tree.
    /// </summary>
    public interface ILogical
    {
        /// <summary>
        /// Raised when the control is attached to a rooted logical tree.
        /// </summary>
        event EventHandler<LogicalTreeAttachmentEventArgs>? AttachedToLogicalTree;

        /// <summary>
        /// Raised when the control is detached from a rooted logical tree.
        /// </summary>
        event EventHandler<LogicalTreeAttachmentEventArgs>? DetachedFromLogicalTree;

        /// <summary>
        /// Raised when the logical children of the control change.
        /// </summary>
        event EventHandler? LogicalChildrenChanged;

        /// <summary>
        /// Gets a value indicating whether the element is attached to a rooted logical tree.
        /// </summary>
        bool IsAttachedToLogicalTree { get; }

        /// <summary>
        /// Gets the logical parent.
        /// </summary>
        ILogical? LogicalParent { get; }

        /// <summary>
        /// Gets the number of logical children of the control.
        /// </summary>
        int LogicalChildrenCount { get; }

        /// <summary>
        /// Returns the specified logical child.
        /// </summary>
        /// <param name="index">
        /// The index of the logical child; must be less than <see cref="LogicalChildrenCount"/>.
        /// </param>
        ILogical GetLogicalChild(int index);

        /// <summary>
        /// Notifies the control that it is being attached to a rooted logical tree.
        /// </summary>
        /// <param name="e">The event args.</param>
        /// <remarks>
        /// This method will be called automatically by the framework, you should not need to call
        /// this method yourself.
        /// </remarks>
        void NotifyAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e);

        /// <summary>
        /// Notifies the control that it is being detached from a rooted logical tree.
        /// </summary>
        /// <param name="e">The event args.</param>
        /// <remarks>
        /// This method will be called automatically by the framework, you should not need to call
        /// this method yourself.
        /// </remarks>
        void NotifyDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e);

        /// <summary>
        /// Notifies the control that a change has been made to resources that apply to it.
        /// </summary>
        /// <param name="e">The event args.</param>
        /// <remarks>
        /// This method will be called automatically by the framework, you should not need to call
        /// this method yourself.
        /// </remarks>
        void NotifyResourcesChanged(ResourcesChangedEventArgs e);
    }
}
