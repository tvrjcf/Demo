using System;
using System.Collections.Generic;
using System.Text;

namespace Py.Collections {

    /// <summary>
    /// 表示一棵树。
    /// </summary>
    /// <typeparam name="T">内容类型。</typeparam>
    public class TreeNode<T> :ITreeNode<T> {

        string _name;

        TreeNode<T> _parentNode;

        TreeNode<T> _next;

        TreeNodeList<T> _childNodes = new TreeNodeList<T>();

        /// <summary>
        /// 初始化 <see cref="Py.Collections.TreeNode&lt;T&gt;"/> 的新实例。
        /// </summary>
        /// <param name="name">节点的名字。</param>
        public TreeNode(string name){
            _name = name;
        }

        /// <summary>
        /// 初始化 <see cref="Py.Collections.TreeNode&lt;T&gt;"/> 的新实例。
        /// </summary>
        public TreeNode() {
            
        }

        /// <summary>
        /// 初始化 <see cref="Py.Collections.TreeNode&lt;T&gt;"/> 的新实例。
        /// </summary>
        /// <param name="name">节点的名字。</param>
        /// <param name="value">节点所在值。</param>
        public TreeNode(string name, T value) {
            _name = name;
            NodeValue = value;
        }

        /// <summary>
        /// 生成当前节点的子节点。
        /// </summary>
        /// <param name="name">节点的名字。</param>
        /// <param name="value">节点所在值。</param>
        /// <returns>节点。</returns>
        public TreeNode<T> CreateSubTreeNode(string name, T value) {
            return new TreeNode<T>(name, value) { ParentNode = this };
        }

        /// <summary>
        /// 获取或设置当前节点值。
        /// </summary>
        public T NodeValue {
            get;
            set;
        }

        /// <summary>
        /// 获取当前树父节点。
        /// </summary>
        public TreeNode<T> ParentNode {
            get { return _parentNode; }
            set { _parentNode = value; }
        }

        /// <summary>
        /// 获取当前节点的深度。
        /// </summary>
        public int Depth {
            get {
                int i = 0;
                ITreeNode<T> a = ParentNode;
                while (a != null) {
                    a = a.ParentNode;
                    i++;
                }

                return i;
            }
        }

        /// <summary>
        /// 获取或设置下一个相邻的节点。
        /// </summary>
        public TreeNode<T> NextSibling {
            get {
                return _next;
            }
            set {
                _next = value;
            }
        }

        /// <summary>
        /// 获取上一个相邻的节点。
        /// </summary>
        public TreeNode<T> PreciousSibling {
            get {
                TreeNode<T> r = null;
                foreach (TreeNode<T> p in _parentNode.ChildNodes) {
                    if (p == this)
                        return r;
                    r = p;
                }
                return null;
            }
        }

        /// <summary>
        /// 获取当前节点的子节点。
        /// </summary>
        public ITreeNodeList<T> ChildNodes {
            get { return _childNodes; }
        }

        /// <summary>
        /// 获取单元的名字。
        /// </summary>
        public string Name {
            get { return _name; }
        }


        #region ITreeNode<T> 成员

        /// <summary>
        /// 获取下一个相邻的节点。
        /// </summary>
        ITreeNode<T> ITreeNode<T>.NextSibling {
            get { return NextSibling; }
        }

        /// <summary>
        /// 获取上一个相邻的节点。
        /// </summary>
        ITreeNode<T> ITreeNode<T>.PreviousSibling {
            get { return PreciousSibling; }
        }

        #endregion

        #region ITreeNode<T> 成员


        ITreeNode<T> ITreeNode<T>.ParentNode {
            get { return ParentNode; }
        }

        #endregion
    }

    /// <summary>
    /// 树节点列表。
    /// </summary>
    /// <typeparam name="T">内容类型。</typeparam>
    public class TreeNodeList<T> :ITreeNodeList<T> {

        /// <summary>
        /// 首个子节点。
        /// </summary>
        TreeNode<T> _firstChild;

        /// <summary>
        /// 获取或设置首节点。
        /// </summary>
        public TreeNode<T> First {
            get {
                return _firstChild;
            }
            set {
                _firstChild = value;
            }
        }

        #region ITreeNodeList<T> 成员

        /// <summary>
        /// 获取指定位置的节点。
        /// </summary>
        /// <param name="i">位置。</param>
        /// <returns>节点。</returns>
        public ITreeNode<T> this[int i] {
            get {
                ITreeNode<T> t = _firstChild;
                while (i-- > 0 && t != null)
                    t = t.NextSibling;
                return t ;
            }
        }

        /// <summary>
        /// 获取指定位置的节点。
        /// </summary>
        /// <param name="name">节点名。</param>
        /// <returns>节点。</returns>
        public ITreeNode<T> this[string name] {
            get {
                for (ITreeNode<T> t = _firstChild; t != null; t = t.NextSibling)
                    if (t.Name == name)
                        return t;
                return null;
            }
        }

        /// <summary>
        /// 获取子节点数目。
        /// </summary>
        public int Count {
            get {
                int i = 0;
                for (ITreeNode<T> t = _firstChild; t != null; t = t.NextSibling)
                    i++;

                return i;
            }
        }

        #endregion

        #region IEnumerable<ITreeNode<T>> 成员

        /// <summary>
        /// 返回一个循环访问集合的枚举数。
        /// </summary>
        /// <returns>
        /// 可用于循环访问集合的  IEnumerator 对象。
        /// </returns>
        public IEnumerator<ITreeNode<T>> GetEnumerator() {
            return new TreeNodeEnumerator(_firstChild);
        }

        #endregion

        #region IEnumerable 成员

        /// <summary>
        /// 返回一个循环访问集合的枚举数。
        /// </summary>
        /// <returns>
        /// 可用于循环访问集合的 <see cref="T:System.Collections.IEnumerator"/> 对象。
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new TreeNodeEnumerator(_firstChild);
        }

        class TreeNodeEnumerator :IEnumerator<ITreeNode<T>> {

            TreeNode<T> _firstNode;

            TreeNode<T> _current;


            public TreeNodeEnumerator(TreeNode<T> firstNode) {
                _current = _firstNode = firstNode;
            }

            #region IEnumerator<ITreeNode<T>> 成员

            public ITreeNode<T> Current {
                get { return _current; }
            }

            #endregion

            #region IDisposable 成员

            public void Dispose() {
                
            }

            #endregion

            #region IEnumerator 成员

            object System.Collections.IEnumerator.Current {
                get { return _current; }
            }

            public bool MoveNext() {
                _current = _current.NextSibling;
                return _current != null;
            }

            public void Reset() {
                _current = _firstNode;
            }

            #endregion
        }

        #endregion
    }
}
