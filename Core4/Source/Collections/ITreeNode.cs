using System;
using System.Collections.Generic;
using System.Text;

namespace Py.Collections {

    /// <summary>
    /// 表示一个树节点。
    /// </summary>
    /// <typeparam name="T">树所带值。</typeparam>
    public interface ITreeNode<T> :IEntry {
        
        /// <summary>
        /// 获取或设置当前节点值。
        /// </summary>
        T NodeValue { get; set; }

        /// <summary>
        /// 获取当前树父节点。
        /// </summary>
        ITreeNode<T> ParentNode { get; }

        /// <summary>
        /// 获取下一个相邻的节点。
        /// </summary>
        ITreeNode<T> NextSibling { get; }

        /// <summary>
        /// 获取上一个相邻的节点。
        /// </summary>
        ITreeNode<T> PreviousSibling { get; }

        /// <summary>
        /// 获取当前节点的深度。
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// 获取当前节点的子节点。
        /// </summary>
        ITreeNodeList<T> ChildNodes { get; }


    }

    /// <summary>
    /// 表示所有子节点。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeNodeList<T> : IEnumerable<ITreeNode<T>> {

        /// <summary>
        /// 获取指定位置的节点。
        /// </summary>
        /// <param name="i">位置。</param>
        /// <returns>节点。</returns>
        ITreeNode<T> this[int i] { get; }

        /// <summary>
        /// 获取指定位置的节点。
        /// </summary>
        /// <param name="name">节点名。</param>
        /// <returns>节点。</returns>
        ITreeNode<T> this[string name] { get; }

        /// <summary>
        /// 获取子节点数目。
        /// </summary>
        int Count { get; }

    }
}
